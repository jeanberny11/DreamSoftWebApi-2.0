using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Auth;

public class AdminLoginCommandHandler(
    IAdminUserRepository adminUserRepository,
    IAdminRefreshTokenRepository adminRefreshTokenRepository,
    IUnitOfWork unitOfWork,
    ICurrentAdminService currentAdminService,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IDateTime dateTime)
    : IRequestHandler<AdminLoginCommand, AdminLoginResponse>
{
    private const int DefaultRefreshTokenExpiryDays = 7;

    public async Task<AdminLoginResponse> Handle(
        AdminLoginCommand request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        // 1. Find admin user — return generic error to prevent enumeration
        var adminUser = await adminUserRepository.GetByEmailAsync(email, cancellationToken)
            ?? throw new UnauthorizedException("Invalid credentials.");

        // 2. Verify password before revealing any account state
        if (!passwordHasher.VerifyPassword(request.Password, adminUser.PasswordHash))
        {
            adminUser.RecordFailedLogin();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException("Invalid credentials.");
        }

        // 3. Check lockout
        if (adminUser.IsLockedOut())
        {
            var remaining = (int)Math.Ceiling(
                (adminUser.LockoutUntil!.Value - dateTime.UtcNow).TotalMinutes);
            throw new ForbiddenException(
                ForbiddenErrorCodes.AccountLocked,
                ErrorMessageKeys.AccountLocked,
                remaining);
        }

        // 4. Check that account is active
        if (!adminUser.IsActive)
            throw new ForbiddenException(
                ForbiddenErrorCodes.TenantSuspended,
                ErrorMessageKeys.AccountSuspended);

        // 5. Successful login — reset lockout, record timestamp
        adminUser.ResetFailedLoginAttempts();
        adminUser.RecordLogin();

        // 6. Issue tokens
        var accessToken   = tokenService.GenerateSuperAdminToken(adminUser);
        var rawToken      = tokenService.GenerateRefreshToken();
        var refreshExpiry = dateTime.UtcNow.AddDays(DefaultRefreshTokenExpiryDays);
        var expiresAt     = dateTime.UtcNow.AddMinutes(60);

        // 7. Persist refresh token
        var refreshTokenEntity = AdminRefreshToken.Create(
            adminUserId:  adminUser.Id,
            token:        rawToken,
            expiresAt:    refreshExpiry,
            createdByIp:  currentAdminService.IpAddress,
            deviceInfo:   request.DeviceInfo);

        await adminRefreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AdminLoginResponse(
            AccessToken:  accessToken,
            RefreshToken: rawToken,
            ExpiresAt:    expiresAt,
            AdminUserId:  adminUser.Id,
            Email:        adminUser.Email,
            FullName:     adminUser.GetFullName(),
            RoleCode:     adminUser.RoleCode);
    }
}

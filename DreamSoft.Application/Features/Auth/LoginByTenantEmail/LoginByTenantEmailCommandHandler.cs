using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RefreshTokenEntity = DreamSoft.Domain.Entities.RefreshToken;

namespace DreamSoft.Application.Features.Auth.LoginByTenantEmail;

public class LoginByTenantEmailCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IDateTime dateTime)
    : IRequestHandler<LoginByTenantEmailCommand, LoginResponse>
{
    private const int DefaultRefreshTokenExpiryDays  = 7;
    private const int ExtendedRefreshTokenExpiryDays = 30;

    public async Task<LoginResponse> Handle(
        LoginByTenantEmailCommand request,
        CancellationToken cancellationToken)
    {
        var tenantEmail = request.TenantEmail.Trim().ToLower();

        // 1. Find the tenant by company email — global query (no tenant filter on Tenants)
        var tenant = await context.Tenants
            .Include(t => t.Status)
            .FirstOrDefaultAsync(
                t => t.Email == tenantEmail,
                cancellationToken)
            ?? throw new UnauthorizedException("Invalid credentials.");

        // 2. Find the user by username within that tenant
        var user = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                u => u.TenantId == tenant.Id &&
                     u.Username == request.Username.Trim() &&
                     u.IsActive,
                cancellationToken)
            ?? throw new UnauthorizedException("Invalid credentials.");

        // 3. Verify password — always check before revealing any account/tenant state
        if (!passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin();
            await context.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException("Invalid credentials.");
        }

        // 4. Credentials are valid — now check tenant state
        switch (tenant.Status.Code)
        {
            case TenantStatusCodes.PendingEmailVerification:
                throw new ForbiddenException(
                    ForbiddenErrorCodes.TenantPendingEmailVerification,
                    "Tenant email address has not been verified.");

            case TenantStatusCodes.PendingSubscription:
                throw new ForbiddenException(
                    ForbiddenErrorCodes.TenantPendingSubscription,
                    "Tenant subscription setup is not complete.");

            case TenantStatusCodes.Suspended:
                throw new ForbiddenException(
                    ForbiddenErrorCodes.TenantSuspended,
                    "Tenant account has been suspended.");

            case TenantStatusCodes.Cancelled:
                throw new ForbiddenException(
                    ForbiddenErrorCodes.TenantCancelled,
                    "Tenant account has been cancelled.");
        }

        // 5. Check user email verification
        if (!user.IsEmailVerified)
            throw new ForbiddenException(
                ForbiddenErrorCodes.EmailNotVerified,
                "Email address has not been verified.");

        // 6. Check account lockout
        if (user.IsLockedOut())
        {
            var remaining = (int)Math.Ceiling(
                (user.LockoutUntil!.Value - dateTime.UtcNow).TotalMinutes);
            throw new ForbiddenException(
                ForbiddenErrorCodes.AccountLocked,
                $"Account is locked. Try again in {remaining} minute(s).");
        }

        // 7. Successful login — reset lockout and record LastLoginAt
        user.ResetFailedLoginAttempts();
        user.RecordLogin();

        // 8. Issue tokens
        var accessToken   = tokenService.GenerateAccessToken(user, tenant);
        var rawToken      = tokenService.GenerateRefreshToken();
        var expiryDays    = request.RememberMe ? ExtendedRefreshTokenExpiryDays : DefaultRefreshTokenExpiryDays;
        var refreshExpiry = dateTime.UtcNow.AddDays(expiryDays);
        var expiresAt     = dateTime.UtcNow.AddMinutes(60);

        // 9. Persist the RefreshToken entity (one row per session — multi-device support)
        var refreshTokenEntity = RefreshTokenEntity.Create(
            userId:      user.Id,
            token:       rawToken,
            expiresAt:   refreshExpiry,
            createdByIp: currentUserService.IpAddress,
            deviceInfo:  request.DeviceInfo);

        context.RefreshTokens.Add(refreshTokenEntity);
        await context.SaveChangesAsync(cancellationToken);

        return new LoginResponse(
            AccessToken:  accessToken,
            RefreshToken: rawToken,
            ExpiresAt:    expiresAt,
            UserId:       user.Id,
            Username:     user.Username,
            FullName:     user.GetFullName());
    }
}

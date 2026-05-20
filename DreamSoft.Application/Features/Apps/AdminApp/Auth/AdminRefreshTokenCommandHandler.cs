using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Auth;

public class AdminRefreshTokenCommandHandler(
    IAdminRefreshTokenRepository adminRefreshTokenRepository,
    IUnitOfWork unitOfWork,
    ICurrentAdminService currentAdminService,
    ITokenService tokenService,
    IDateTime dateTime)
    : IRequestHandler<AdminRefreshTokenCommand, AdminLoginResponse>
{
    private const int RefreshTokenExpiryDays = 7;

    public async Task<AdminLoginResponse> Handle(
        AdminRefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var existingToken = await adminRefreshTokenRepository
            .GetActiveTokenAsync(request.RefreshToken, cancellationToken)
            ?? throw new UnauthorizedException("Invalid or expired refresh token.");

        // Revoke the old token (token rotation)
        existingToken.Revoke(currentAdminService.IpAddress);

        var adminUser = existingToken.AdminUser;

        if (!adminUser.IsActive)
            throw new ForbiddenException(
                ForbiddenErrorCodes.TenantSuspended,
                ErrorMessageKeys.AccountSuspended);

        // Issue new tokens
        var accessToken   = tokenService.GenerateSuperAdminToken(adminUser);
        var rawToken      = tokenService.GenerateRefreshToken();
        var refreshExpiry = dateTime.UtcNow.AddDays(RefreshTokenExpiryDays);
        var expiresAt     = dateTime.UtcNow.AddMinutes(60);

        var newRefreshToken = AdminRefreshToken.Create(
            adminUserId:  adminUser.Id,
            token:        rawToken,
            expiresAt:    refreshExpiry,
            createdByIp:  currentAdminService.IpAddress);

        await adminRefreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
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

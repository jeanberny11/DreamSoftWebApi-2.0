using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Auth.RefreshToken;

public class RefreshTokenCommandHandler(
    ITenantRepository tenantRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ITokenService tokenService,
    IDateTime dateTime)
    : IRequestHandler<RefreshTokenCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Look up the RefreshToken entity with its user — no separate user table scan needed
        var tokenEntity = await refreshTokenRepository.GetActiveByTokenWithUserAsync(
            request.RefreshToken, cancellationToken)
            ?? throw new UnauthorizedException("Invalid or expired refresh token.");

        // 2. Validate user is still active
        if (!tokenEntity.User.IsActive)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        // 3. Load the tenant to generate a valid access token
        var tenant = await tenantRepository.GetByIdAsync(tokenEntity.User.TenantId, cancellationToken)
            ?? throw new UnauthorizedException("Invalid or expired refresh token.");

        // 4. Revoke the consumed token (record which IP triggered rotation)
        tokenEntity.Revoke(currentUserService.IpAddress);

        // 5. Preserve the original session window so RememberMe sessions keep their
        //    30-day expiry even after rotation
        var originalWindow   = tokenEntity.ExpiresAt - tokenEntity.CreatedAt;
        var newRefreshExpiry = dateTime.UtcNow.Add(originalWindow);

        // 6. Issue a new token pair
        var newAccessToken = tokenService.GenerateAccessToken(tokenEntity.User, tenant);
        var newRawToken    = tokenService.GenerateRefreshToken();
        var expiresAt      = dateTime.UtcNow.AddMinutes(60);

        var newTokenEntity = Domain.Entities.RefreshToken.Create(
            userId:      tokenEntity.User.Id,
            token:       newRawToken,
            expiresAt:   newRefreshExpiry,
            createdByIp: currentUserService.IpAddress,
            deviceInfo:  tokenEntity.DeviceInfo);

        await refreshTokenRepository.AddAsync(newTokenEntity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponse(
            AccessToken:     newAccessToken,
            RefreshToken:    newRawToken,
            ExpiresAt:       expiresAt,
            UserId:          tokenEntity.User.Id,
            Username:        tokenEntity.User.Username,
            FullName:        tokenEntity.User.GetFullName(),
            TenantSubdomain: tenant.Subdomain);
    }
}

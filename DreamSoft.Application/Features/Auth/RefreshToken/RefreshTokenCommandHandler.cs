using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RefreshTokenEntity = DreamSoft.Domain.Entities.RefreshToken;

namespace DreamSoft.Application.Features.Auth.RefreshToken;

public class RefreshTokenCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    ITokenService tokenService,
    IDateTime dateTime)
    : IRequestHandler<RefreshTokenCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Look up the RefreshToken entity directly — no user table scan needed.
        //    Include the user so we can generate an access token.
        var tokenEntity = await context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(
                rt => rt.Token == request.RefreshToken,
                cancellationToken)
            ?? throw new UnauthorizedException("Invalid or expired refresh token.");

        // 2. Validate token state (not expired, not revoked, user still active)
        if (!tokenEntity.IsActive || !tokenEntity.User.IsActive)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        // 3. Load the tenant to generate a valid access token
        var tenant = await context.Tenants
            .FirstOrDefaultAsync(t => t.Id == tokenEntity.User.TenantId, cancellationToken)
            ?? throw new UnauthorizedException("Invalid or expired refresh token.");

        // 4. Revoke the consumed token (record which IP triggered rotation)
        tokenEntity.Revoke(currentUserService.IpAddress);

        // 5. Preserve the original session window so RememberMe sessions keep their
        //    30-day expiry even after rotation (rather than resetting to a short window).
        var originalWindow    = tokenEntity.ExpiresAt - tokenEntity.CreatedAt;
        var newRefreshExpiry  = dateTime.UtcNow.Add(originalWindow);

        // 6. Issue a new token pair
        var newAccessToken  = tokenService.GenerateAccessToken(tokenEntity.User, tenant);
        var newRawToken     = tokenService.GenerateRefreshToken();
        var expiresAt       = dateTime.UtcNow.AddMinutes(60);

        var newTokenEntity = RefreshTokenEntity.Create(
            userId:      tokenEntity.User.Id,
            token:       newRawToken,
            expiresAt:   newRefreshExpiry,
            createdByIp: currentUserService.IpAddress,
            deviceInfo:  tokenEntity.DeviceInfo);

        context.RefreshTokens.Add(newTokenEntity);
        await context.SaveChangesAsync(cancellationToken);

        return new LoginResponse(
            AccessToken:      newAccessToken,
            RefreshToken:     newRawToken,
            ExpiresAt:        expiresAt,
            UserId:           tokenEntity.User.Id,
            Username:         tokenEntity.User.Username,
            FullName:         tokenEntity.User.GetFullName(),
            TenantSubdomain:  tenant.Subdomain);
    }
}

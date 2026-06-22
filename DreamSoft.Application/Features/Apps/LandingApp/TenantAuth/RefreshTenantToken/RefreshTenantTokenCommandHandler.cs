using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using DreamSoft.Application.Features.Apps.LandingApp.TenantAuth.LoginTenant;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.TenantAuth.RefreshTenantToken;

public class RefreshTenantTokenCommandHandler(
    ITenantRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenantService,
    ITokenService tokenService,
    IDateTime dateTime)
    : IRequestHandler<RefreshTenantTokenCommand, RefreshTenantTokenResponse>
{
    public async Task<RefreshTenantTokenResponse> Handle(
        RefreshTenantTokenCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw new UnauthorizedException(ErrorMessageKeys.InvalidRefreshToken);

        // 1. Look up the TenantRefreshToken entity with its tenant navigation loaded
        var tokenEntity = await refreshTokenRepository.GetActiveTokenAsync(
            request.RefreshToken, cancellationToken)
            ?? throw new UnauthorizedException(ErrorMessageKeys.InvalidRefreshToken);

        // 2. Guard against suspended / cancelled tenants
        var statusCode = tokenEntity.Tenant.Status?.Code;
        if (statusCode is TenantStatusCodes.Suspended or TenantStatusCodes.Cancelled)
            throw new UnauthorizedException(ErrorMessageKeys.InvalidRefreshToken);

        // 3. Revoke the consumed token (record which IP triggered rotation)
        tokenEntity.Revoke(currentTenantService.IpAddress);

        // 4. Preserve the original session window so RememberMe sessions keep their
        //    30-day expiry even after rotation
        var originalWindow = tokenEntity.ExpiresAt - tokenEntity.CreatedAt;
        var newRefreshExpiry = dateTime.UtcNow.Add(originalWindow);

        // 5. Issue a new token pair
        var newAccessToken = tokenService.GenerateTenantAccessToken(tokenEntity.Tenant);
        var newRawToken = tokenService.GenerateRefreshToken();
        var expiresAt = dateTime.UtcNow.AddMinutes(60);

        var newTokenEntity = TenantRefreshToken.Create(
            tenantId: tokenEntity.TenantId,
            token: newRawToken,
            expiresAt: newRefreshExpiry,
            createdByIp: currentTenantService.IpAddress,
            deviceInfo: tokenEntity.DeviceInfo);

        await refreshTokenRepository.AddAsync(newTokenEntity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RefreshTenantTokenResponse(
            AccessToken:         newAccessToken,
            RefreshToken:        newRawToken,
            ExpiresAt:           expiresAt,
            TenantId:            tokenEntity.Tenant.Id,
            Email:               tokenEntity.Tenant.Email,
            FirstName:           tokenEntity.Tenant.FirstName,
            LastName:            tokenEntity.Tenant.LastName,
            LogoUrl:             tokenEntity.Tenant.LogoUrl,
            TenantStatus:        tokenEntity.Tenant.Status?.Code ?? string.Empty,
            EmailVerified:       tokenEntity.Tenant.EmailVerified,
            OnboardingCompleted: tokenEntity.Tenant.OnboardingCompleted);
    }
}

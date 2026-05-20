using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.TenantAuth.LogoutTenant;

public class LogoutTenantCommandHandler(
    ITenantRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<LogoutTenantCommand>
{
    public async Task Handle(LogoutTenantCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenantService.TenantId
            ?? throw new UnauthorizedException(ErrorMessageKeys.Unauthorized);

        var ip = currentTenantService.IpAddress;

        if (request.RefreshToken is not null)
        {
            // Targeted logout — revoke only the specific session token
            var tokenEntity = await refreshTokenRepository.GetActiveTokenAsync(
                request.RefreshToken, cancellationToken);

            // Silently succeed if already revoked / not found (idempotent)
            if (tokenEntity is { IsRevoked: false } && tokenEntity.TenantId == tenantId)
            {
                tokenEntity.Revoke(ip);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
        else
        {
            // Global logout — revoke all active sessions for this tenant
            await refreshTokenRepository.RevokeAllForTenantAsync(tenantId, ip, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}

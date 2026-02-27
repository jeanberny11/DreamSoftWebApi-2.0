using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Auth.Logout;

public class LogoutCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("Unauthorized.");

        var ip = currentUserService.IpAddress;

        if (request.RefreshToken is not null)
        {
            // Targeted logout — revoke only the specific session token
            var tokenEntity = await refreshTokenRepository.GetActiveByTokenAsync(
                request.RefreshToken, cancellationToken);

            // Silently succeed if already revoked / not found (idempotent)
            if (tokenEntity is { IsRevoked: false } && tokenEntity.UserId == userId)
            {
                tokenEntity.Revoke(ip);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
        else
        {
            // Global logout — revoke all active sessions for this user
            var activeTokens = await refreshTokenRepository.GetActiveByUserAsync(
                userId, cancellationToken);

            foreach (var token in activeTokens)
                token.Revoke(ip);

            if (activeTokens.Count > 0)
                await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}

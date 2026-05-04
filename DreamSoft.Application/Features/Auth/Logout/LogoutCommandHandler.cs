using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Auth.Logout;

public class LogoutCommandHandler(
    IUserRefreshTokenRepository refreshTokenRepository,
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
            var tokenEntity = await refreshTokenRepository.GetActiveTokenAsync(
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
            await refreshTokenRepository.RevokeAllForUserAsync(userId, ip, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}

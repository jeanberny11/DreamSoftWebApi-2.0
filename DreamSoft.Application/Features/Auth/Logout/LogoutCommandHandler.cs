using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Application.Features.Auth.Logout;

public class LogoutCommandHandler(
    IApplicationDbContext context,
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
            var tokenEntity = await context.RefreshTokens
                .FirstOrDefaultAsync(
                    rt => rt.Token == request.RefreshToken && rt.UserId == userId,
                    cancellationToken);

            // Silently succeed if already revoked / not found (idempotent)
            if (tokenEntity is { IsRevoked: false })
            {
                tokenEntity.Revoke(ip);
                await context.SaveChangesAsync(cancellationToken);
            }
        }
        else
        {
            // Global logout — revoke all active sessions for this user
            var activeTokens = await context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            foreach (var token in activeTokens)
                token.Revoke(ip);

            if (activeTokens.Count > 0)
                await context.SaveChangesAsync(cancellationToken);
        }
    }
}

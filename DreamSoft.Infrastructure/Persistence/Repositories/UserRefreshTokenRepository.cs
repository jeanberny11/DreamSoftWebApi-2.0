using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class UserRefreshTokenRepository(ApplicationDbContext context)
    : Repository<UserRefreshToken>(context), IUserRefreshTokenRepository
{
    public async Task<UserRefreshToken?> GetActiveTokenAsync(string token, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token
                && rt.RevokedAt == null
                && rt.ExpiresAt > DateTime.UtcNow, cancellationToken);

    public async Task RevokeAllForUserAsync(int userId, string? revokedByIp, CancellationToken cancellationToken = default)
    {
        var activeTokens = await _dbSet
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
            token.Revoke(revokedByIp);
    }
}

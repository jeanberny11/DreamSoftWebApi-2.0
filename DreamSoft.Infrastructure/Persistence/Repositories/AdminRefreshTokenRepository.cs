using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class AdminRefreshTokenRepository(ApplicationDbContext context)
    : Repository<AdminRefreshToken>(context), IAdminRefreshTokenRepository
{
    public async Task<AdminRefreshToken?> GetActiveTokenAsync(string token, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(rt => rt.AdminUser)
            .FirstOrDefaultAsync(rt => rt.Token == token
                && rt.RevokedAt == null
                && rt.ExpiresAt > DateTime.UtcNow, cancellationToken);

    public async Task RevokeAllForAdminAsync(int adminUserId, string? revokedByIp, CancellationToken cancellationToken = default)
    {
        var activeTokens = await _dbSet
            .Where(rt => rt.AdminUserId == adminUserId && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
            token.Revoke(revokedByIp);
    }
}

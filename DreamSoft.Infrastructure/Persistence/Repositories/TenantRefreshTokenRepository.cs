using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class TenantRefreshTokenRepository(ApplicationDbContext context)
    : Repository<TenantRefreshToken>(context), ITenantRefreshTokenRepository
{
    public async Task<TenantRefreshToken?> GetActiveTokenAsync(string token, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(rt => rt.Tenant)
                .ThenInclude(t => t.Status)
            .FirstOrDefaultAsync(rt => rt.Token == token
                && rt.RevokedAt == null
                && rt.ExpiresAt > DateTime.UtcNow, cancellationToken);

    public async Task RevokeAllForTenantAsync(int tenantId, string? revokedByIp, CancellationToken cancellationToken = default)
    {
        var activeTokens = await _dbSet
            .Where(rt => rt.TenantId == tenantId && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
            token.Revoke(revokedByIp);
    }
}

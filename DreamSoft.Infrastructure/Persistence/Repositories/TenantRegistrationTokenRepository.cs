using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class TenantRegistrationTokenRepository(ApplicationDbContext context)
    : Repository<TenantRegistrationToken>(context), ITenantRegistrationTokenRepository
{
    public async Task<TenantRegistrationToken?> GetActiveTokenForTenantAsync(int tenantId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(t => t.TenantId == tenantId && !t.IsConsumed && t.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task ConsumeAllForTenantAsync(int tenantId, CancellationToken cancellationToken = default)
    {
        var activeTokens = await _dbSet
            .Where(t => t.TenantId == tenantId && !t.IsConsumed)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
            token.Consume();
    }

    public async Task<bool> HasRecentTokenAsync(int tenantId, int minutesAgo, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-minutesAgo);
        return await _dbSet
            .AnyAsync(t => t.TenantId == tenantId && t.CreatedAt >= cutoff, cancellationToken);
    }
}

using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class TenantRegistrationTokenRepository(ApplicationDbContext context)
    : Repository<TenantRegistrationToken>(context), ITenantRegistrationTokenRepository
{
    public async Task<TenantRegistrationToken?> GetActiveByTenantAsync(int tenantId, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(t =>
            t.TenantId == tenantId &&
            !t.IsConsumed &&
            t.ExpiresAt > DateTime.UtcNow, ct);

    public async Task ConsumeAllByTenantAsync(int tenantId, CancellationToken ct = default)
    {
        var tokens = await _dbSet
            .Where(t => t.TenantId == tenantId && !t.IsConsumed)
            .ToListAsync(ct);

        foreach (var token in tokens)
            token.Consume();
    }
}

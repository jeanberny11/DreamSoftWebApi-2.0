using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class TenantSubscriptionRepository(ApplicationDbContext context)
    : Repository<TenantSubscription>(context), ITenantSubscriptionRepository
{
    public async Task<TenantSubscription?> GetActiveByTenantAsync(int tenantId, CancellationToken ct = default)
        => await _dbSet
            .Include(s => s.Status)
            .FirstOrDefaultAsync(s =>
                s.TenantId == tenantId &&
                (s.EndDate == null || s.EndDate > DateTime.UtcNow), ct);

    public async Task<IReadOnlyList<TenantSubscription>> GetAllByTenantAsync(int tenantId, CancellationToken ct = default)
        => await _dbSet
            .Include(s => s.Status)
            .Include(s => s.Solution)
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.TenantId == tenantId)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync(ct);
}

using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class SubscriptionCancellationLogRepository(ApplicationDbContext context)
    : Repository<SubscriptionCancellationLog>(context), ISubscriptionCancellationLogRepository
{
    public async Task<IReadOnlyList<SubscriptionCancellationLog>> GetBySubscriptionIdAsync(
        int tenantSubscriptionId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(l => l.TenantSubscriptionId == tenantSubscriptionId)
            .OrderByDescending(l => l.CancelledAt)
            .ToListAsync(cancellationToken);
}

using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class SubscriptionPlanRepository(ApplicationDbContext context)
    : Repository<SubscriptionPlan>(context), ISubscriptionPlanRepository
{
    public async Task<IReadOnlyList<SubscriptionPlan>> GetActiveBySolutionAsync(int solutionId, CancellationToken ct = default)
        => await _dbSet
            .Include(p => p.BillingCycle)
            .Where(p => p.SolutionId == solutionId && p.IsActive)
            .ToListAsync(ct);

    public async Task<SubscriptionPlan?> GetByIdWithBillingCycleAsync(int planId, CancellationToken ct = default)
        => await _dbSet
            .Include(p => p.BillingCycle)
            .FirstOrDefaultAsync(p => p.Id == planId, ct);
}

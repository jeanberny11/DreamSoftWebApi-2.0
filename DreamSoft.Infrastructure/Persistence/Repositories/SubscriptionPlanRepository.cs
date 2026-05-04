using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class SubscriptionPlanRepository(ApplicationDbContext context)
    : Repository<SubscriptionPlan>(context), ISubscriptionPlanRepository
{
    public async Task<IReadOnlyList<SubscriptionPlan>> GetBySolutionIdAsync(int solutionId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(p => p.SolutionId == solutionId && p.IsActive)
            .OrderBy(p => p.TierLevel)
            .ToListAsync(cancellationToken);

    public async Task<SubscriptionPlan?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(p => p.Code == code.ToUpper().Trim(), cancellationToken);

    public async Task<SubscriptionPlan?> GetWithPricesAndLimitsAsync(int id, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(p => p.PlanPrices)
                .ThenInclude(pp => pp.BillingCycle)
            .Include(p => p.PlanLimits)
            .Include(p => p.PlanMenuOptions)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
}

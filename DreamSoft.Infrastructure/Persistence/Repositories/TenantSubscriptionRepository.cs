using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class TenantSubscriptionRepository(ApplicationDbContext context)
    : Repository<TenantSubscription>(context), ITenantSubscriptionRepository
{
    public async Task<TenantSubscription?> GetByIdWithStatusAsync(int id, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(ts => ts.SubscriptionPlan)
            .Include(ts => ts.PlanPrice).ThenInclude(pp => pp.BillingCycle)
            .Include(ts => ts.Status)
            .FirstOrDefaultAsync(ts => ts.Id == id, cancellationToken);

    public async Task<TenantSubscription?> GetByTenantAndSolutionAsync(int tenantId, int solutionId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(ts => ts.SubscriptionPlan)
            .Include(ts => ts.PlanPrice).ThenInclude(pp => pp.BillingCycle)
            .Include(ts => ts.Status)
            .FirstOrDefaultAsync(ts => ts.TenantId == tenantId && ts.SolutionId == solutionId, cancellationToken);

    public async Task<IReadOnlyList<TenantSubscription>> GetActiveByTenantAsync(int tenantId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(ts => ts.SubscriptionPlan)
            .Include(ts => ts.Solution)
            .Where(ts => ts.TenantId == tenantId && ts.IsActive)
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsForTenantAndSolutionAsync(int tenantId, int solutionId, CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(ts => ts.TenantId == tenantId && ts.SolutionId == solutionId, cancellationToken);

    public async Task<TenantSubscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(ts => ts.SubscriptionPlan)
            .Include(ts => ts.PlanPrice).ThenInclude(pp => pp.BillingCycle)
            .Include(ts => ts.Status)
            .FirstOrDefaultAsync(ts => ts.StripeSubscriptionId == stripeSubscriptionId, cancellationToken);

    public async Task<TenantSubscription?> GetByStripeSessionIdAsync(string stripeSessionId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(ts => ts.SubscriptionPlan)
            .Include(ts => ts.PlanPrice).ThenInclude(pp => pp.BillingCycle)
            .Include(ts => ts.Status)
            .FirstOrDefaultAsync(ts => ts.StripeSessionId == stripeSessionId, cancellationToken);
}

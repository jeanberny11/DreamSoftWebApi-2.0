using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class SubscriptionInvoiceRepository(ApplicationDbContext context)
    : Repository<SubscriptionInvoice>(context), ISubscriptionInvoiceRepository
{
    /// <summary>Shared include chain for display-ready invoice queries (Solution, translated Plan, nested Payments).</summary>
    private IQueryable<SubscriptionInvoice> WithDetails()
        => _dbSet
            .Include(i => i.TenantSubscription).ThenInclude(ts => ts.Solution)
            .Include(i => i.TenantSubscription).ThenInclude(ts => ts.SubscriptionPlan).ThenInclude(sp => sp.Translations)
            .Include(i => i.Payments);

    public async Task<IReadOnlyList<SubscriptionInvoice>> GetByTenantIdAsync(int tenantId, CancellationToken cancellationToken = default)
        => await WithDetails()
            .Where(i => i.TenantId == tenantId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<SubscriptionInvoice?> GetByStripeInvoiceIdAsync(string stripeInvoiceId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(i => i.TenantSubscription)
                .ThenInclude(ts => ts.SubscriptionPlan)
            .Include(i => i.TenantSubscription)
                .ThenInclude(ts => ts.PlanPrice)
                    .ThenInclude(pp => pp.BillingCycle)
            .FirstOrDefaultAsync(i => i.StripeInvoiceId == stripeInvoiceId, cancellationToken);

    public async Task<IReadOnlyList<SubscriptionInvoice>> GetByTenantAndSolutionIdAsync(
        int tenantId, int solutionId, CancellationToken cancellationToken = default)
        => await WithDetails()
            .Where(i => i.TenantId == tenantId && i.TenantSubscription.SolutionId == solutionId)
            .OrderByDescending(i => i.DueDate)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SubscriptionInvoice>> GetByTenantAndDateRangeAsync(
        int tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        => await WithDetails()
            .Where(i => i.TenantId == tenantId && i.DueDate >= startDate && i.DueDate <= endDate)
            .OrderByDescending(i => i.DueDate)
            .ToListAsync(cancellationToken);

    public async Task<SubscriptionInvoice?> GetByTenantAndStripeInvoiceIdAsync(
        int tenantId, string stripeInvoiceId, CancellationToken cancellationToken = default)
        => await WithDetails()
            .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.StripeInvoiceId == stripeInvoiceId, cancellationToken);
}

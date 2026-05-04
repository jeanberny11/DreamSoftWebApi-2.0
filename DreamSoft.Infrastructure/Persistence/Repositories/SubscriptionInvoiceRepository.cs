using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class SubscriptionInvoiceRepository(ApplicationDbContext context)
    : Repository<SubscriptionInvoice>(context), ISubscriptionInvoiceRepository
{
    public async Task<IReadOnlyList<SubscriptionInvoice>> GetByTenantIdAsync(int tenantId, CancellationToken cancellationToken = default)
        => await _dbSet
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
}

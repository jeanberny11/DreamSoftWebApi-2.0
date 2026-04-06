using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ISubscriptionInvoiceRepository : IRepository<SubscriptionInvoice>
{
    Task<IReadOnlyList<SubscriptionInvoice>> GetByTenantIdAsync(int tenantId, CancellationToken cancellationToken = default);
    Task<SubscriptionInvoice?> GetByStripeInvoiceIdAsync(string stripeInvoiceId, CancellationToken cancellationToken = default);
}

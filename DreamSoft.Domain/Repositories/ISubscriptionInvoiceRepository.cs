using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ISubscriptionInvoiceRepository : IRepository<SubscriptionInvoice>
{
    Task<IReadOnlyList<SubscriptionInvoice>> GetByTenantIdAsync(int tenantId, CancellationToken cancellationToken = default);
    Task<SubscriptionInvoice?> GetByStripeInvoiceIdAsync(string stripeInvoiceId, CancellationToken cancellationToken = default);

    /// <summary>Invoices for a tenant scoped to a specific solution.</summary>
    Task<IReadOnlyList<SubscriptionInvoice>> GetByTenantAndSolutionIdAsync(
        int tenantId, int solutionId, CancellationToken cancellationToken = default);

    /// <summary>Invoices for a tenant with a DueDate falling within the given range (inclusive).</summary>
    Task<IReadOnlyList<SubscriptionInvoice>> GetByTenantAndDateRangeAsync(
        int tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>A single invoice for a tenant matching a specific Stripe invoice ID.</summary>
    Task<SubscriptionInvoice?> GetByTenantAndStripeInvoiceIdAsync(
        int tenantId, string stripeInvoiceId, CancellationToken cancellationToken = default);
}

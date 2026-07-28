using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ISubscriptionPaymentRepository : IRepository<SubscriptionPayment>
{
    Task<bool> ExistsByStripePaymentIdAndStatusAsync(
        string stripePaymentId,
        string status,
        CancellationToken cancellationToken = default);

    /// <summary>All payment attempts for a tenant, with display details, optionally scoped to one solution.</summary>
    Task<IReadOnlyList<SubscriptionPayment>> GetByTenantIdWithDetailsAsync(
        int tenantId, int? solutionId = null, CancellationToken cancellationToken = default);

    /// <summary>Payment attempts for a tenant scoped to a specific solution.</summary>
    Task<IReadOnlyList<SubscriptionPayment>> GetByTenantAndSolutionIdAsync(
        int tenantId, int solutionId, CancellationToken cancellationToken = default);

    /// <summary>Payment attempts for a tenant with a PaymentDate falling within the given range (inclusive).</summary>
    Task<IReadOnlyList<SubscriptionPayment>> GetByTenantAndDateRangeAsync(
        int tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>Payment attempts for a tenant scoped to a specific invoice.</summary>
    Task<IReadOnlyList<SubscriptionPayment>> GetByTenantAndInvoiceIdAsync(
        int tenantId, int invoiceId, CancellationToken cancellationToken = default);
}

using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ITenantSubscriptionRepository : IRepository<TenantSubscription>
{
    Task<TenantSubscription?> GetByIdWithStatusAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a single subscription by ID with all navigations needed for
    /// full DTO projection: Solution, SubscriptionPlan (+Translations),
    /// PlanPrice → BillingCycle (+Translations), and Status.
    /// Ownership is NOT checked here — callers must verify TenantId.
    /// </summary>
    Task<TenantSubscription?> GetByIdWithDetailsAsync(
        int id,
        CancellationToken cancellationToken = default);
    Task<TenantSubscription?> GetByTenantAndSolutionAsync(int tenantId, int solutionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantSubscription>> GetActiveByTenantAsync(int tenantId, CancellationToken cancellationToken = default);
    Task<bool> ExistsForTenantAndSolutionAsync(int tenantId, int solutionId, CancellationToken cancellationToken = default);
    Task<TenantSubscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the exact subscription that was created for a specific Stripe
    /// checkout session. This is the safest way to correlate a
    /// checkout.session.completed webhook to its local subscription record —
    /// one session ID maps to exactly one subscription.
    /// </summary>
    Task<TenantSubscription?> GetByStripeSessionIdAsync(string stripeSessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all subscriptions for a tenant, excluding any subscription whose
    /// status code matches <paramref name="excludedStatusCode"/>. Includes Solution,
    /// SubscriptionPlan, PlanPrice.BillingCycle, and Status for projection.
    /// </summary>
    Task<IReadOnlyList<TenantSubscription>> GetByTenantExcludingStatusAsync(
        int tenantId,
        string excludedStatusCode,
        CancellationToken cancellationToken = default);
}

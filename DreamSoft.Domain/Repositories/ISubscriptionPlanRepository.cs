using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ISubscriptionPlanRepository : IRepository<SubscriptionPlan>
{
    Task<IReadOnlyList<SubscriptionPlan>> GetBySolutionIdAsync(int solutionId, CancellationToken cancellationToken = default);
    Task<SubscriptionPlan?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<SubscriptionPlan?> GetWithPricesAndLimitsAsync(int id, CancellationToken cancellationToken = default);
    Task<SubscriptionPlan?> GetWithSolutionPricesAndLimitsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a plan with its Solution, Translations, and PlanPrices
    /// (with BillingCycle + Translations) — everything a plan-change
    /// preview needs to render the "new plan" side in one query.
    /// </summary>
    Task<SubscriptionPlan?> GetByIdWithSolutionAndPricesAsync(
        int id, CancellationToken cancellationToken = default);
}

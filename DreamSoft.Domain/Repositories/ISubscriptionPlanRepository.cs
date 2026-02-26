using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ISubscriptionPlanRepository : IRepository<SubscriptionPlan>
{
    Task<IReadOnlyList<SubscriptionPlan>> GetActiveBySolutionAsync(int solutionId, CancellationToken ct = default);
    Task<SubscriptionPlan?> GetByIdWithBillingCycleAsync(int planId, CancellationToken ct = default);
}

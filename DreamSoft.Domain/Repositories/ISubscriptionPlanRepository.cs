using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ISubscriptionPlanRepository : IRepository<SubscriptionPlan>
{
    Task<IReadOnlyList<SubscriptionPlan>> GetBySolutionIdAsync(int solutionId, CancellationToken cancellationToken = default);
    Task<SubscriptionPlan?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<SubscriptionPlan?> GetWithPricesAndLimitsAsync(int id, CancellationToken cancellationToken = default);
}

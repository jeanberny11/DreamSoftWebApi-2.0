using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ISubscriptionCancellationLogRepository : IRepository<SubscriptionCancellationLog>
{
    /// <summary>
    /// Returns all cancellation log entries for a given subscription,
    /// ordered by most recent first.
    /// </summary>
    Task<IReadOnlyList<SubscriptionCancellationLog>> GetBySubscriptionIdAsync(
        int tenantSubscriptionId,
        CancellationToken cancellationToken = default);
}

using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ISubscriptionStatusRepository : IRepository<SubscriptionStatus>
{
    Task<SubscriptionStatus?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}

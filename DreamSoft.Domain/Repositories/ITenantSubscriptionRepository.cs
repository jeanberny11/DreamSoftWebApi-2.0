using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ITenantSubscriptionRepository : IRepository<TenantSubscription>
{
    Task<TenantSubscription?> GetActiveByTenantAsync(int tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<TenantSubscription>> GetAllByTenantAsync(int tenantId, CancellationToken ct = default);
}

using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ITenantRegistrationTokenRepository : IRepository<TenantRegistrationToken>
{
    Task<TenantRegistrationToken?> GetActiveByTenantAsync(int tenantId, CancellationToken ct = default);
    Task ConsumeAllByTenantAsync(int tenantId, CancellationToken ct = default);
    Task<bool> HasRecentTokenAsync(int tenantId, int minutesAgo, CancellationToken ct = default);
}

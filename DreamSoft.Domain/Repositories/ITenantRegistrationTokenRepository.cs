using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ITenantRegistrationTokenRepository : IRepository<TenantRegistrationToken>
{
    Task<TenantRegistrationToken?> GetActiveTokenForTenantAsync(int tenantId, CancellationToken cancellationToken = default);
    Task ConsumeAllForTenantAsync(int tenantId, CancellationToken cancellationToken = default);
    Task<bool> HasRecentTokenAsync(int tenantId, int minutesAgo, CancellationToken cancellationToken = default);
}

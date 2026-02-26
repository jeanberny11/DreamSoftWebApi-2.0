using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ITenantRepository : IRepository<Tenant>
{
    Task<bool> ExistsBySubdomainAsync(string subdomain, CancellationToken ct = default);
    Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken ct = default);
    Task<Tenant?> GetBySubdomainWithStatusAsync(string subdomain, CancellationToken ct = default);
}

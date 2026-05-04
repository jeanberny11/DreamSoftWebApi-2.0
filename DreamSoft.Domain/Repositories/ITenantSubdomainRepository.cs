using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ITenantSubdomainRepository : IRepository<TenantSubdomain>
{
    Task<TenantSubdomain?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default);
    Task<TenantSubdomain?> GetByTenantAndSolutionAsync(int tenantId, int solutionId, CancellationToken cancellationToken = default);
    Task<bool> SubdomainExistsAsync(string subdomain, CancellationToken cancellationToken = default);
}

using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ITenantSubdomainRepository : IRepository<TenantSubdomain>
{
    Task<TenantSubdomain?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default);
    Task<TenantSubdomain?> GetByTenantAndSolutionAsync(int tenantId, int solutionId, CancellationToken cancellationToken = default);
    Task<bool> SubdomainExistsAsync(string subdomain, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all subdomains provisioned for a tenant, across all solutions.
    /// Used to bulk-resolve subdomains when listing a tenant's subscriptions,
    /// avoiding one query per solution.
    /// </summary>
    Task<IReadOnlyList<TenantSubdomain>> GetByTenantIdAsync(
        int tenantId,
        CancellationToken cancellationToken = default);
}

using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class TenantSubdomainRepository(ApplicationDbContext context)
    : Repository<TenantSubdomain>(context), ITenantSubdomainRepository
{
    public async Task<TenantSubdomain?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(td => td.Tenant)
            .FirstOrDefaultAsync(td => td.Subdomain == subdomain.ToLower().Trim(), cancellationToken);

    public async Task<TenantSubdomain?> GetByTenantAndSolutionAsync(int tenantId, int solutionId, CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(td => td.TenantId == tenantId && td.SolutionId == solutionId, cancellationToken);

    public async Task<bool> SubdomainExistsAsync(string subdomain, CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(td => td.Subdomain == subdomain.ToLower().Trim(), cancellationToken);
}

using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class TenantRepository(ApplicationDbContext context)
    : Repository<Tenant>(context), ITenantRepository
{
    public async Task<bool> ExistsBySubdomainAsync(string subdomain, CancellationToken ct = default)
        => await _dbSet.AnyAsync(t => t.Subdomain == subdomain.ToLower().Trim(), ct);

    public async Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(t => t.Subdomain == subdomain.ToLower().Trim(), ct);

    public async Task<Tenant?> GetBySubdomainWithStatusAsync(string subdomain, CancellationToken ct = default)
        => await _dbSet
            .Include(t => t.Status)
            .FirstOrDefaultAsync(t => t.Subdomain == subdomain.ToLower().Trim(), ct);
}

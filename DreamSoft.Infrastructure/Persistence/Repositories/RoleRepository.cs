using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class RoleRepository(ApplicationDbContext context)
    : Repository<Role>(context), IRoleRepository
{
    public async Task<IReadOnlyList<Role>> GetByTenantAsync(int tenantId, CancellationToken ct = default)
        => await _dbSet
            .Where(r => r.TenantId == tenantId)
            .ToListAsync(ct);

    public async Task<Role?> GetByCodeAndTenantAsync(string code, int tenantId, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(r =>
            r.Code == code.ToUpper().Trim() &&
            r.TenantId == tenantId, ct);
}

using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class RoleRepository(ApplicationDbContext context)
    : Repository<Role>(context), IRoleRepository
{
    public async Task<Role?> GetByCodeAsync(int tenantId, int solutionId, string code, CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(r => r.TenantId == tenantId
                && r.SolutionId == solutionId
                && r.Code == code.ToUpper().Trim(), cancellationToken);

    public async Task<Role?> GetWithPermissionsAsync(int id, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(r => r.RoleMenuOptions)
            .Include(r => r.RoleOptionActions)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Role>> GetAllByTenantAndSolutionAsync(int tenantId, int solutionId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(r => r.TenantId == tenantId && r.SolutionId == solutionId)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
}

using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class RoleTemplateRepository(ApplicationDbContext context)
    : Repository<RoleTemplate>(context), IRoleTemplateRepository
{
    public async Task<IReadOnlyList<RoleTemplate>> GetByPlanIdAsync(int planId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(rt => rt.PlanId == planId)
            .OrderBy(rt => rt.Name)
            .ToListAsync(cancellationToken);

    public async Task<RoleTemplate?> GetWithMenuOptionsAsync(int id, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(rt => rt.RoleMenuOptionTemplates)
            .Include(rt => rt.RoleOptionActionTemplates)
            .FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);
}

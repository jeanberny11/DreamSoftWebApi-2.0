using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class PlanLimitRepository(ApplicationDbContext context)
    : Repository<PlanLimit>(context), IPlanLimitRepository
{
    public async Task<IReadOnlyList<PlanLimit>> GetByPlanIdAsync(
        int planId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(pl => pl.PlanId == planId)
            .OrderBy(pl => pl.LimitKey)
            .ToListAsync(cancellationToken);

    public async Task<PlanLimit?> GetByPlanAndKeyAsync(
        int planId, string limitKey, CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(pl => pl.PlanId == planId
                && pl.LimitKey == limitKey.ToLower().Trim(), cancellationToken);
}

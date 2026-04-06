using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class PlanMenuOptionRepository(ApplicationDbContext context)
    : IPlanMenuOptionRepository
{
    private readonly DbSet<PlanMenuOption> _dbSet = context.Set<PlanMenuOption>();

    public async Task<IReadOnlyList<PlanMenuOption>> GetByPlanIdAsync(int planId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(pm => pm.MenuOption)
            .Where(pm => pm.PlanId == planId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<int>> GetMenuOptionIdsByPlanIdAsync(int planId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(pm => pm.PlanId == planId)
            .Select(pm => pm.MenuOptionId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(PlanMenuOption entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<PlanMenuOption> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    public Task DeleteAsync(PlanMenuOption entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }
}

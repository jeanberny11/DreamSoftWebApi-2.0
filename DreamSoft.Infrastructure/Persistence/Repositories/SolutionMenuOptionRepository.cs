using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class SolutionMenuOptionRepository(ApplicationDbContext context) : ISolutionMenuOptionRepository
{
    private readonly DbSet<SolutionMenuOption> _dbSet = context.Set<SolutionMenuOption>();

    public async Task<IReadOnlyList<SolutionMenuOption>> GetBySolutionAsync(int solutionId, CancellationToken ct = default)
        => await _dbSet
            .Include(s => s.MenuOption)
            .Where(s => s.SolutionId == solutionId)
            .ToListAsync(ct);

    public async Task AddAsync(SolutionMenuOption entity, CancellationToken ct = default)
        => await _dbSet.AddAsync(entity, ct);

    public async Task AddRangeAsync(IEnumerable<SolutionMenuOption> entities, CancellationToken ct = default)
        => await _dbSet.AddRangeAsync(entities, ct);

    public Task DeleteAsync(SolutionMenuOption entity, CancellationToken ct = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }
}

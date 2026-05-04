using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IPlanMenuOptionRepository
{
    Task<IReadOnlyList<PlanMenuOption>> GetByPlanIdAsync(int planId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetMenuOptionIdsByPlanIdAsync(int planId, CancellationToken cancellationToken = default);
    Task AddAsync(PlanMenuOption entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<PlanMenuOption> entities, CancellationToken cancellationToken = default);
    Task DeleteAsync(PlanMenuOption entity, CancellationToken cancellationToken = default);
}

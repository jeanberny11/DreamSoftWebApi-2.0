using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ISolutionMenuOptionRepository
{
    Task<IReadOnlyList<SolutionMenuOption>> GetBySolutionAsync(int solutionId, CancellationToken ct = default);
    Task AddAsync(SolutionMenuOption entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<SolutionMenuOption> entities, CancellationToken ct = default);
    Task DeleteAsync(SolutionMenuOption entity, CancellationToken ct = default);
}

using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IPlanLimitRepository : IRepository<PlanLimit>
{
    Task<IReadOnlyList<PlanLimit>> GetByPlanIdAsync(int planId, CancellationToken cancellationToken = default);
    Task<PlanLimit?> GetByPlanAndKeyAsync(int planId, string limitKey, CancellationToken cancellationToken = default);
}

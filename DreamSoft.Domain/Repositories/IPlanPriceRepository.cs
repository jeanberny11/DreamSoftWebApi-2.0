using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IPlanPriceRepository : IRepository<PlanPrice>
{
    Task<IReadOnlyList<PlanPrice>> GetByPlanIdAsync(int planId, CancellationToken cancellationToken = default);
}

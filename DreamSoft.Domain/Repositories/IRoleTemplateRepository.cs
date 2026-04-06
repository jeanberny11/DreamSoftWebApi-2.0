using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IRoleTemplateRepository : IRepository<RoleTemplate>
{
    Task<IReadOnlyList<RoleTemplate>> GetByPlanIdAsync(int planId, CancellationToken cancellationToken = default);
    Task<RoleTemplate?> GetWithMenuOptionsAsync(int id, CancellationToken cancellationToken = default);
}

using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IRoleOptionActionRepository : IRepository<RoleOptionAction>
{
    Task<IReadOnlyList<RoleOptionAction>> GetByRoleAsync(int roleId, CancellationToken ct = default);
}

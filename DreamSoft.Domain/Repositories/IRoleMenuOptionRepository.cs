using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IRoleMenuOptionRepository : IRepository<RoleMenuOption>
{
    Task<IReadOnlyList<RoleMenuOption>> GetByRoleAsync(int roleId, CancellationToken ct = default);
}

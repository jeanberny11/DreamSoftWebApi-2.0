using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IRoleMenuOptionRepository : IRepository<RoleMenuOption>
{
    Task<IReadOnlyList<RoleMenuOption>> GetByRoleIdAsync(int roleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoleMenuOption>> GetByRoleIdWithMenuDataAsync(int roleId, CancellationToken cancellationToken = default);
    Task ReplaceForRoleAsync(int roleId, IEnumerable<RoleMenuOption> menuOptions, CancellationToken cancellationToken = default);
}

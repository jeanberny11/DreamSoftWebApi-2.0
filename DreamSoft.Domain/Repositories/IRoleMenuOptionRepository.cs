using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IRoleMenuOptionRepository : IRepository<RoleMenuOption>
{
    Task<IReadOnlyList<RoleMenuOption>> GetByRoleAsync(int roleId, CancellationToken ct = default);
    Task AddAsync(RoleMenuOption entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<RoleMenuOption> entities, CancellationToken ct = default);
    Task DeleteAsync(RoleMenuOption entity, CancellationToken ct = default);
}

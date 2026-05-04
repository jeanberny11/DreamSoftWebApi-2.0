using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IRoleOptionActionRepository : IRepository<RoleOptionAction>
{
    Task<IReadOnlyList<RoleOptionAction>> GetByRoleIdAsync(int roleId, CancellationToken cancellationToken = default);
    Task ReplaceForRoleAsync(int roleId, IEnumerable<RoleOptionAction> actions, CancellationToken cancellationToken = default);
}

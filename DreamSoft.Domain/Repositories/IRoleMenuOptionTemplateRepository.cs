using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IRoleMenuOptionTemplateRepository : IRepository<RoleMenuOptionTemplate>
{
    Task<IReadOnlyList<RoleMenuOptionTemplate>> GetByTemplateIdAsync(int roleTemplateId, CancellationToken cancellationToken = default);
}

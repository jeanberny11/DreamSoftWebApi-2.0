using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IRoleMenuOptionTemplateRepository : IRepository<RoleMenuOptionTemplate>
{
    Task<IReadOnlyList<RoleMenuOptionTemplate>> GetByTemplateAsync(int roleTemplateId, CancellationToken ct = default);
}

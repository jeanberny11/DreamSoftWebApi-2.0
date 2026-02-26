using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IRoleOptionActionTemplateRepository : IRepository<RoleOptionActionTemplate>
{
    Task<IReadOnlyList<RoleOptionActionTemplate>> GetByTemplateAsync(int roleTemplateId, CancellationToken ct = default);
}

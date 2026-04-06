using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IRoleOptionActionTemplateRepository : IRepository<RoleOptionActionTemplate>
{
    Task<IReadOnlyList<RoleOptionActionTemplate>> GetByTemplateIdAsync(int roleTemplateId, CancellationToken cancellationToken = default);
}

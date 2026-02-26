using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IRoleTemplateRepository : IRepository<RoleTemplate>
{
    Task<RoleTemplate?> GetByCodeAsync(string code, CancellationToken ct = default);
}

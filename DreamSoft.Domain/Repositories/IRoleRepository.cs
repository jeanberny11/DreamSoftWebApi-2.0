using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IRoleRepository : IRepository<Role>
{
    Task<IReadOnlyList<Role>> GetByTenantAsync(int tenantId, CancellationToken ct = default);
    Task<Role?> GetByCodeAndTenantAsync(string code, int tenantId, CancellationToken ct = default);
}

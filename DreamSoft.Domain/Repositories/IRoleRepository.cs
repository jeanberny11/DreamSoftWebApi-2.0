using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByCodeAsync(int tenantId, int solutionId, string code, CancellationToken cancellationToken = default);
    Task<Role?> GetWithPermissionsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Role>> GetAllByTenantAndSolutionAsync(int tenantId, int solutionId, CancellationToken cancellationToken = default);
}

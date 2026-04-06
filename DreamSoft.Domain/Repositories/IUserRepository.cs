using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(int tenantId, int solutionId, string username, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(int tenantId, int solutionId, string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdWithRoleAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(int tenantId, int solutionId, string username, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(int tenantId, int solutionId, string email, CancellationToken cancellationToken = default);
    Task<int> CountByTenantAndSolutionAsync(int tenantId, int solutionId, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAndTenantAsync(int tenantId, string username, CancellationToken cancellationToken = default);
    Task<User?> GetAdminByTenantAsync(int tenantId, CancellationToken cancellationToken = default);
}

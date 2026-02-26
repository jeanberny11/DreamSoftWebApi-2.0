using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<bool> ExistsByEmailGloballyAsync(string email, CancellationToken ct = default);
    Task<User?> GetByUsernameAndTenantAsync(string username, int tenantId, CancellationToken ct = default);
    Task<User?> GetAdminByTenantAsync(int tenantId, CancellationToken ct = default);
    Task<User?> GetByIdGlobalAsync(int userId, CancellationToken ct = default);
}

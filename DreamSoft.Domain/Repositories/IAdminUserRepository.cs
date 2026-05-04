using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IAdminUserRepository : IRepository<AdminUser>
{
    Task<AdminUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}

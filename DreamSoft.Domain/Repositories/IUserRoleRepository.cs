using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IUserRoleRepository
{
    Task<IReadOnlyList<UserRole>> GetByUserAsync(int userId, CancellationToken ct = default);
    Task<IReadOnlyList<UserRole>> GetByRoleAsync(int roleId, CancellationToken ct = default);
    Task AddAsync(UserRole entity, CancellationToken ct = default);
    Task DeleteAsync(UserRole entity, CancellationToken ct = default);
}

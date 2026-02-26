using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class UserRepository(ApplicationDbContext context)
    : Repository<User>(context), IUserRepository
{
    public async Task<bool> ExistsByEmailGloballyAsync(string email, CancellationToken ct = default)
        => await _dbSet
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == email.Trim().ToLower(), ct);

    public async Task<User?> GetByUsernameAndTenantAsync(string username, int tenantId, CancellationToken ct = default)
        => await _dbSet
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u =>
                u.TenantId == tenantId &&
                u.Username == username.Trim().ToLower() &&
                u.IsActive, ct);

    public async Task<User?> GetAdminByTenantAsync(int tenantId, CancellationToken ct = default)
        => await _dbSet
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u =>
                u.TenantId == tenantId &&
                u.IsAdmin &&
                u.IsActive, ct);

    public async Task<User?> GetByIdGlobalAsync(int userId, CancellationToken ct = default)
        => await _dbSet
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
}

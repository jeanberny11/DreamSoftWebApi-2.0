using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class UserRoleRepository(ApplicationDbContext context) : IUserRoleRepository
{
    private readonly DbSet<UserRole> _dbSet = context.Set<UserRole>();

    public async Task<IReadOnlyList<UserRole>> GetByUserAsync(int userId, CancellationToken ct = default)
        => await _dbSet
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<UserRole>> GetByRoleAsync(int roleId, CancellationToken ct = default)
        => await _dbSet
            .Include(ur => ur.User)
            .Where(ur => ur.RoleId == roleId)
            .ToListAsync(ct);

    public async Task AddAsync(UserRole entity, CancellationToken ct = default)
        => await _dbSet.AddAsync(entity, ct);

    public Task DeleteAsync(UserRole entity, CancellationToken ct = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }
}

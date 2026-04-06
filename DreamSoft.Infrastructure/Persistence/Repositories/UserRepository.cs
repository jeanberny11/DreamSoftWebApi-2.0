using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class UserRepository(ApplicationDbContext applicationDbContext)
    : Repository<User>(applicationDbContext), IUserRepository
{
    public async Task<User?> GetByUsernameAsync(int tenantId, int solutionId, string username, CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(u => u.TenantId == tenantId
                && u.SolutionId == solutionId
                && u.Username == username.ToLower().Trim(), cancellationToken);

    public async Task<User?> GetByEmailAsync(int tenantId, int solutionId, string email, CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(u => u.TenantId == tenantId
                && u.SolutionId == solutionId
                && u.Email == email.ToLower().Trim(), cancellationToken);

    public async Task<User?> GetByIdWithRoleAsync(int id, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(u => u.Role)
                .ThenInclude(r => r!.RoleMenuOptions)
            .Include(u => u.Role)
                .ThenInclude(r => r!.RoleOptionActions)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<bool> UsernameExistsAsync(int tenantId, int solutionId, string username, CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(u => u.TenantId == tenantId
            && u.SolutionId == solutionId
            && u.Username == username.ToLower().Trim(), cancellationToken);

    public async Task<bool> EmailExistsAsync(int tenantId, int solutionId, string email, CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(u => u.TenantId == tenantId
            && u.SolutionId == solutionId
            && u.Email == email.ToLower().Trim(), cancellationToken);

    public async Task<int> CountByTenantAndSolutionAsync(int tenantId, int solutionId, CancellationToken cancellationToken = default)
        => await _dbSet.CountAsync(u => u.TenantId == tenantId && u.SolutionId == solutionId, cancellationToken);

    public async Task<User?> GetByUsernameAndTenantAsync(int tenantId, string username, CancellationToken cancellationToken = default)
        => await _dbSet
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.TenantId == tenantId
                && u.Username == username.ToLower().Trim(), cancellationToken);

    public async Task<User?> GetAdminByTenantAsync(int tenantId, CancellationToken cancellationToken = default)
        => await _dbSet
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.IsAdmin, cancellationToken);
}

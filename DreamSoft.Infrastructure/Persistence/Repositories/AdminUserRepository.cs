using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class AdminUserRepository(ApplicationDbContext context)
    : Repository<AdminUser>(context), IAdminUserRepository
{
    public async Task<AdminUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(a => a.Email == email.Trim().ToLowerInvariant(), cancellationToken);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        => await _dbSet
            .AnyAsync(a => a.Email == email.Trim().ToLowerInvariant(), cancellationToken);
}

using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class RoleMenuOptionRepository(ApplicationDbContext context)
    : Repository<RoleMenuOption>(context), IRoleMenuOptionRepository
{
    public async Task<IReadOnlyList<RoleMenuOption>> GetByRoleAsync(int roleId, CancellationToken ct = default)
        => await _dbSet.Where(r => r.RoleId == roleId).ToListAsync(ct);
}

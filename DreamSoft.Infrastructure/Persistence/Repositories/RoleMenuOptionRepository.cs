using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class RoleMenuOptionRepository(ApplicationDbContext context)
    : Repository<RoleMenuOption>(context), IRoleMenuOptionRepository
{
    public async Task<IReadOnlyList<RoleMenuOption>> GetByRoleIdAsync(int roleId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(rm => rm.RoleId == roleId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<RoleMenuOption>> GetByRoleIdWithMenuDataAsync(int roleId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(rm => rm.RoleId == roleId)
            .Include(rm => rm.MenuOption)
                .ThenInclude(mo => mo.Module)
            .Include(rm => rm.MenuOption)
                .ThenInclude(mo => mo.MenuGroup)
            .ToListAsync(cancellationToken);

    public async Task ReplaceForRoleAsync(int roleId, IEnumerable<RoleMenuOption> menuOptions, CancellationToken cancellationToken = default)
    {
        var existing = await _dbSet
            .Where(rm => rm.RoleId == roleId)
            .ToListAsync(cancellationToken);

        _dbSet.RemoveRange(existing);
        await _dbSet.AddRangeAsync(menuOptions, cancellationToken);
    }
}

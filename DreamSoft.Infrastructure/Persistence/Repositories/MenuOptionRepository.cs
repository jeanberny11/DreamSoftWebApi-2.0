using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class MenuOptionRepository(ApplicationDbContext context)
    : Repository<MenuOption>(context), IMenuOptionRepository
{
    public async Task<IReadOnlyList<MenuOption>> GetAllActiveAsync(
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(mo => mo.IsActive)
            .OrderBy(mo => mo.SortOrder)
            .ToListAsync(cancellationToken);

    public async Task<MenuOption?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(m => m.Code == code.ToUpper().Trim(), cancellationToken);

    public async Task<IReadOnlyList<MenuOption>> GetAllActiveWithModuleAndGroupAsync(CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(mo => mo.IsActive)
            .Include(mo => mo.Module)
            .Include(mo => mo.MenuGroup)
            .OrderBy(mo => mo.SortOrder)
            .ToListAsync(cancellationToken);
}

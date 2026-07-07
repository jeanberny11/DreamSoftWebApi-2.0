using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class ModuleRepository(ApplicationDbContext context)
    : Repository<Module>(context), IModuleRepository
{
    public async Task<IReadOnlyList<Module>> GetAllActiveAsync(
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(m => m.IsActive)
            .OrderBy(m => m.SortOrder)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Module>> GetAllActiveWithMenuOptionsAsync(
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(m => m.IsActive)
            .Include(m => m.MenuOptions.Where(mo => mo.IsActive))
            .OrderBy(m => m.SortOrder)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Module>> GetActiveByCodesWithMenuOptionsAsync(
        IEnumerable<string> codes,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(m => m.IsActive && codes.Contains(m.Code))
            .Include(m => m.MenuOptions.Where(mo => mo.IsActive))
                .ThenInclude(mo => mo.MenuGroup)
            .OrderBy(m => m.SortOrder)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Module>> GetAllActiveWithMenuOptionsAndGroupsAsync(
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(m => m.IsActive)
            .Include(m => m.MenuOptions.Where(mo => mo.IsActive))
                .ThenInclude(mo => mo.MenuGroup)
            .OrderBy(m => m.SortOrder)
            .ToListAsync(cancellationToken);
}

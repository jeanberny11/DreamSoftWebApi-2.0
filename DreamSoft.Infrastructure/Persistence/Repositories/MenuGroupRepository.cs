using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class MenuGroupRepository(ApplicationDbContext context)
    : Repository<MenuGroup>(context), IMenuGroupRepository
{
    public async Task<IReadOnlyList<MenuGroup>> GetAllActiveAsync(
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(mg => mg.IsActive)
            .OrderBy(mg => mg.SortOrder)
            .ToListAsync(cancellationToken);
}

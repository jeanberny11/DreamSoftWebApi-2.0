using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class RoleOptionActionRepository(ApplicationDbContext context)
    : Repository<RoleOptionAction>(context), IRoleOptionActionRepository
{
    public async Task<IReadOnlyList<RoleOptionAction>> GetByRoleIdAsync(int roleId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(ra => ra.RoleId == roleId)
            .ToListAsync(cancellationToken);

    public async Task ReplaceForRoleAsync(int roleId, IEnumerable<RoleOptionAction> actions, CancellationToken cancellationToken = default)
    {
        var existing = await _dbSet
            .Where(ra => ra.RoleId == roleId)
            .ToListAsync(cancellationToken);

        _dbSet.RemoveRange(existing);
        await _dbSet.AddRangeAsync(actions, cancellationToken);
    }
}

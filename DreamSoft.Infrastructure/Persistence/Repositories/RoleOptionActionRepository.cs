using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class RoleOptionActionRepository(ApplicationDbContext context)
    : Repository<RoleOptionAction>(context), IRoleOptionActionRepository
{
    public async Task<IReadOnlyList<RoleOptionAction>> GetByRoleAsync(int roleId, CancellationToken ct = default)
        => await _dbSet.Where(r => r.RoleId == roleId).ToListAsync(ct);
}

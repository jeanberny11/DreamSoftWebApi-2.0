using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class RoleMenuOptionTemplateRepository(ApplicationDbContext context)
    : Repository<RoleMenuOptionTemplate>(context), IRoleMenuOptionTemplateRepository
{
    public async Task<IReadOnlyList<RoleMenuOptionTemplate>> GetByTemplateIdAsync(int roleTemplateId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(rm => rm.RoleTemplateId == roleTemplateId)
            .ToListAsync(cancellationToken);
}

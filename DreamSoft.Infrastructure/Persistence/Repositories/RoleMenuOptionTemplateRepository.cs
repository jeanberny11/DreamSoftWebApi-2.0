using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class RoleMenuOptionTemplateRepository(ApplicationDbContext context)
    : Repository<RoleMenuOptionTemplate>(context), IRoleMenuOptionTemplateRepository
{
    public async Task<IReadOnlyList<RoleMenuOptionTemplate>> GetByTemplateAsync(int roleTemplateId, CancellationToken ct = default)
        => await _dbSet.Where(r => r.RoleId == roleTemplateId).ToListAsync(ct);
}

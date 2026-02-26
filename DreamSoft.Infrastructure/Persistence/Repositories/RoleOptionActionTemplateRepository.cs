using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class RoleOptionActionTemplateRepository(ApplicationDbContext context)
    : Repository<RoleOptionActionTemplate>(context), IRoleOptionActionTemplateRepository
{
    public async Task<IReadOnlyList<RoleOptionActionTemplate>> GetByTemplateAsync(int roleTemplateId, CancellationToken ct = default)
        => await _dbSet.Where(r => r.RoleId == roleTemplateId).ToListAsync(ct);
}

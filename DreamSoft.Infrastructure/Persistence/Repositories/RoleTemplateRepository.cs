using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class RoleTemplateRepository(ApplicationDbContext context)
    : Repository<RoleTemplate>(context), IRoleTemplateRepository
{
    public async Task<RoleTemplate?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(r => r.Code == code.ToUpper().Trim(), ct);
}

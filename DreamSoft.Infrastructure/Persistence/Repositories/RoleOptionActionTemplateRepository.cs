using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class RoleOptionActionTemplateRepository(ApplicationDbContext context)
    : Repository<RoleOptionActionTemplate>(context), IRoleOptionActionTemplateRepository
{
    public async Task<IReadOnlyList<RoleOptionActionTemplate>> GetByTemplateIdAsync(int roleTemplateId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(ra => ra.RoleTemplateId == roleTemplateId)
            .ToListAsync(cancellationToken);
}

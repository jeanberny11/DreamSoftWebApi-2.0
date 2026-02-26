using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class SolutionRepository(ApplicationDbContext context)
    : Repository<Solution>(context), ISolutionRepository
{
    public async Task<IReadOnlyList<Solution>> GetAllActiveAsync(CancellationToken ct = default)
        => await _dbSet.Where(s => s.IsActive).ToListAsync(ct);
}

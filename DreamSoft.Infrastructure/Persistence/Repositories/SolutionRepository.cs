using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class SolutionRepository(ApplicationDbContext context)
    : Repository<Solution>(context), ISolutionRepository
{
    public async Task<Solution?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(s => s.Code == code.ToUpper().Trim(), cancellationToken);

    public async Task<IReadOnlyList<Solution>> GetAllActiveAsync(CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(s => s.IsActive)
            .OrderBy(s => s.SortOrder)
            .ToListAsync(cancellationToken);
}

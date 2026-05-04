using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class TaxClassificationRepository(ApplicationDbContext context)
    : Repository<TaxClassification>(context), ITaxClassificationRepository
{
    public async Task<IReadOnlyList<TaxClassification>> GetAllActiveAsync(
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(tc => tc.IsActive)
            .OrderBy(tc => tc.Code)
            .ToListAsync(cancellationToken);

    public async Task<TaxClassification?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(tc => tc.Code == code.ToUpper().Trim(), cancellationToken);
}

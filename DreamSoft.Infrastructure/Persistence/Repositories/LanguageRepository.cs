using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class LanguageRepository(ApplicationDbContext context)
    : Repository<Language>(context), ILanguageRepository
{
    public async Task<Language?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(l => l.Code == code.ToLower().Trim(), cancellationToken);

    public async Task<Language?> GetDefaultAsync(CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(l => l.IsDefault, cancellationToken);

    public async Task<IReadOnlyList<Language>> GetAllActiveAsync(
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(l => l.IsActive)
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken);
}

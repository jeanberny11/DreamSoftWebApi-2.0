using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class GenderRepository(ApplicationDbContext context)
    : Repository<Gender>(context), IGenderRepository
{
    public async Task<IReadOnlyList<Gender>> GetAllActiveAsync(
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(g => g.IsActive)
            .OrderBy(g => g.Name)
            .ToListAsync(cancellationToken);
}

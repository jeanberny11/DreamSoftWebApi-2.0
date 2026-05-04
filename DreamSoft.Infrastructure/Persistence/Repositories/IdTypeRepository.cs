using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class IdTypeRepository(ApplicationDbContext context)
    : Repository<IdType>(context), IIdTypeRepository
{
    public async Task<IReadOnlyList<IdType>> GetByCountryIdAsync(int countryId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(i => i.CountryId == countryId)
            .OrderBy(i => i.Name)
            .ToListAsync(cancellationToken);
}

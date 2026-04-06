using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class ProvinceRepository(ApplicationDbContext context)
    : Repository<Province>(context), IProvinceRepository
{
    public async Task<IReadOnlyList<Province>> GetByCountryIdAsync(int countryId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(p => p.CountryId == countryId)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
}

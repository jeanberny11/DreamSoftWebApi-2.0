using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class CountryRepository(ApplicationDbContext context)
    : Repository<Country>(context), ICountryRepository
{
    public async Task<Country?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(c => c.Code == code.ToUpper().Trim(), cancellationToken);
}

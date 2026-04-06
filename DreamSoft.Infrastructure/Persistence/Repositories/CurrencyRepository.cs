using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class CurrencyRepository(ApplicationDbContext context)
    : Repository<Currency>(context), ICurrencyRepository
{
    public async Task<Currency?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(c => c.Code == code.ToUpper().Trim(), cancellationToken);
}

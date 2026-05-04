using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class MunicipalityRepository(ApplicationDbContext context)
    : Repository<Municipality>(context), IMunicipalityRepository
{
    public async Task<IReadOnlyList<Municipality>> GetByProvinceIdAsync(int provinceId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(m => m.ProvinceId == provinceId)
            .OrderBy(m => m.Name)
            .ToListAsync(cancellationToken);
}

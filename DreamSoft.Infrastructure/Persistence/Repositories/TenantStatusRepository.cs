using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class TenantStatusRepository(ApplicationDbContext context)
    : Repository<TenantStatus>(context), ITenantStatusRepository
{
    public async Task<TenantStatus?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(ts => ts.Code == code.ToUpper().Trim(), cancellationToken);
}

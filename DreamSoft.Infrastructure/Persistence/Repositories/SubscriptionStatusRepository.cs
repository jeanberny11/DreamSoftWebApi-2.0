using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class SubscriptionStatusRepository(ApplicationDbContext context)
    : Repository<SubscriptionStatus>(context), ISubscriptionStatusRepository
{
    public async Task<SubscriptionStatus?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(ss => ss.Code == code.ToUpper().Trim(), cancellationToken);
}

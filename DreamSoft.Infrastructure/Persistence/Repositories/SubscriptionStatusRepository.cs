using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class SubscriptionStatusRepository(ApplicationDbContext context)
    : Repository<SubscriptionStatus>(context), ISubscriptionStatusRepository
{
    public async Task<SubscriptionStatus?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(s => s.Code == code, ct);
}

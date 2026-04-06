using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class PlanPriceRepository(ApplicationDbContext context)
    : Repository<PlanPrice>(context), IPlanPriceRepository
{
    public async Task<IReadOnlyList<PlanPrice>> GetByPlanIdAsync(int planId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(pp => pp.BillingCycle)
            .Where(pp => pp.PlanId == planId && pp.IsActive)
            .ToListAsync(cancellationToken);
}

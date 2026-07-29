using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class SolutionRepository(ApplicationDbContext context)
    : Repository<Solution>(context), ISolutionRepository
{
    public async Task<Solution?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(s => s.Code == code.ToUpper().Trim(), cancellationToken);

    public async Task<Solution?> GetByCodeWithPlansAsync(string code, CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(s => s.Code == code.ToUpper().Trim())
            .Include(s => s.SubscriptionPlans)
                .ThenInclude(sp => sp.PlanPrices)
                    .ThenInclude(pp => pp.BillingCycle)
            .Include(s => s.SubscriptionPlans)
                .ThenInclude(sp => sp.PlanLimits)
            .Include(s => s.SubscriptionPlans)
                .ThenInclude(sp => sp.PlanMenuOptions)
                    .ThenInclude(pmo => pmo.MenuOption)
                        .ThenInclude(mo => mo.Module)
            .Include(s => s.SubscriptionPlans)
                .ThenInclude(sp => sp.PlanMenuOptions)
                    .ThenInclude(pmo => pmo.MenuOption)
                        .ThenInclude(mo => mo.MenuGroup)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<Solution>> GetAllActiveAsync(CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(s => s.IsActive)
            .OrderBy(s => s.SortOrder)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Solution>> GetAllActiveWithModulesAsync(CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(s => s.IsActive)
            .OrderBy(s => s.SortOrder)
            .Include(s => s.SubscriptionPlans)
                .ThenInclude(sp => sp.PlanMenuOptions)
                    .ThenInclude(pmo => pmo.MenuOption)
                        .ThenInclude(mo => mo.Module)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Solution>> GetAllActiveWithPlansAsync(
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(s => s.IsActive)
            .Include(s => s.SubscriptionPlans.Where(sp => sp.IsActive).OrderBy(sp => sp.TierLevel))
                .ThenInclude(sp => sp.PlanPrices)
                    .ThenInclude(pp => pp.BillingCycle)
            .Include(s => s.SubscriptionPlans)
                .ThenInclude(sp => sp.PlanLimits)
            .OrderBy(s => s.SortOrder)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Solution>> GetAllActiveWithPlansAndFeaturesAsync(
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(s => s.IsActive)
            .Include(s => s.SubscriptionPlans.Where(sp => sp.IsActive).OrderBy(sp => sp.TierLevel))
                .ThenInclude(sp => sp.PlanPrices)
                    .ThenInclude(pp => pp.BillingCycle)
            .Include(s => s.SubscriptionPlans)
                .ThenInclude(sp => sp.PlanLimits)
            .Include(s => s.SubscriptionPlans)
                .ThenInclude(sp => sp.PlanMenuOptions)
                    .ThenInclude(pmo => pmo.MenuOption)
                        .ThenInclude(mo => mo.Module)
            .Include(s => s.SubscriptionPlans)
                .ThenInclude(sp => sp.PlanMenuOptions)
                    .ThenInclude(pmo => pmo.MenuOption)
                        .ThenInclude(mo => mo.MenuGroup)
            .OrderBy(s => s.SortOrder)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

    public async Task<Solution?> GetActiveByCodeWithPlansAndFeaturesAsync(
        string code,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(s => s.IsActive && s.Code == code.ToUpper().Trim())
            .Include(s => s.SubscriptionPlans.Where(sp => sp.IsActive).OrderBy(sp => sp.TierLevel))
                .ThenInclude(sp => sp.PlanPrices)
                    .ThenInclude(pp => pp.BillingCycle)
            .Include(s => s.SubscriptionPlans)
                .ThenInclude(sp => sp.PlanLimits)
            .Include(s => s.SubscriptionPlans)
                .ThenInclude(sp => sp.PlanMenuOptions)
                    .ThenInclude(pmo => pmo.MenuOption)
                        .ThenInclude(mo => mo.Module)
            .Include(s => s.SubscriptionPlans)
                .ThenInclude(sp => sp.PlanMenuOptions)
                    .ThenInclude(pmo => pmo.MenuOption)
                        .ThenInclude(mo => mo.MenuGroup)
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken);
}

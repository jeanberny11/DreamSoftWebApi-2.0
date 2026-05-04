using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class TenantRepository(ApplicationDbContext context)
    : Repository<Tenant>(context), ITenantRepository
{
    public async Task<Tenant?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(t => t.Email == email.ToLower().Trim(), cancellationToken);

    public async Task<Tenant?> GetByEmailWithStatusAsync(string email, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(t => t.Status)
            .FirstOrDefaultAsync(t => t.Email == email.ToLower().Trim(), cancellationToken);

    public async Task<Tenant?> GetByIdWithStatusAsync(int id, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(t => t.Status)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<Tenant?> GetByIdWithSubscriptionsAsync(int id, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(t => t.TenantSubscriptions)
                .ThenInclude(ts => ts.SubscriptionPlan)
            .Include(t => t.TenantSubdomains)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<Tenant?> GetBySubdomainWithStatusAsync(string subdomain, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(t => t.Status)
            .Include(t => t.TenantSubdomains)
            .FirstOrDefaultAsync(
                t => t.TenantSubdomains.Any(ts => ts.Subdomain == subdomain && ts.IsActive),
                cancellationToken);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(t => t.Email == email.ToLower().Trim(), cancellationToken);

    public async Task<Tenant?> GetByStripeCustomerIdAsync(string stripeCustomerId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(t => t.Status)
            .FirstOrDefaultAsync(t => t.StripeCustomerId == stripeCustomerId, cancellationToken);
}

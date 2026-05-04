using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ITenantRepository : IRepository<Tenant>
{
    Task<Tenant?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Tenant?> GetByEmailWithStatusAsync(string email, CancellationToken cancellationToken = default);
    Task<Tenant?> GetByIdWithStatusAsync(int id, CancellationToken cancellationToken = default);
    Task<Tenant?> GetBySubdomainWithStatusAsync(string subdomain, CancellationToken cancellationToken = default);
    Task<Tenant?> GetByIdWithSubscriptionsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<Tenant?> GetByStripeCustomerIdAsync(string stripeCustomerId, CancellationToken cancellationToken = default);
}

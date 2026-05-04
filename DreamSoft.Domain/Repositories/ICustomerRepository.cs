using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<IReadOnlyList<Customer>> GetByTenantAndSolutionAsync(
        int tenantId,
        int solutionId,
        CancellationToken cancellationToken = default);

    Task<Customer?> GetByIdWithDetailsAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByTaxIdAsync(
        int tenantId,
        int solutionId,
        string taxId,
        CancellationToken cancellationToken = default);
}

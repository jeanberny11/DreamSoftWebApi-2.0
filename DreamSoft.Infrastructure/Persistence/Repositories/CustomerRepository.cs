using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class CustomerRepository(ApplicationDbContext context)
    : Repository<Customer>(context), ICustomerRepository
{
    public async Task<IReadOnlyList<Customer>> GetByTenantAndSolutionAsync(
        int tenantId,
        int solutionId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(c => c.CustomerType)
            .Include(c => c.CustomerStatus)
            .Where(c => c.TenantId == tenantId && c.SolutionId == solutionId && c.IsActive)
            .OrderBy(c => c.CompanyName)
            .ThenBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync(cancellationToken);

    public async Task<Customer?> GetByIdWithDetailsAsync(
        int id,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(c => c.CustomerType)
            .Include(c => c.CustomerStatus)
            .Include(c => c.IdType)
            .Include(c => c.TaxClassification)
            .Include(c => c.Currency)
            .Include(c => c.Country)
            .Include(c => c.Province)
            .Include(c => c.Municipality)
            .Include(c => c.Solution)
            .Include(c => c.CreatedByUser)
            .Include(c => c.UpdatedByUser)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<bool> ExistsByTaxIdAsync(
        int tenantId,
        int solutionId,
        string taxId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .AnyAsync(c => c.TenantId == tenantId
                        && c.SolutionId == solutionId
                        && c.TaxId == taxId.Trim(),
                cancellationToken);
}

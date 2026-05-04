using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class CustomerStatusRepository(ApplicationDbContext context)
    : Repository<CustomerStatus>(context), ICustomerStatusRepository
{
    public async Task<IReadOnlyList<CustomerStatus>> GetAllActiveAsync(
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(cs => cs.IsActive)
            .OrderBy(cs => cs.Name)
            .ToListAsync(cancellationToken);
}

using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class CustomerTypeRepository(ApplicationDbContext context)
    : Repository<CustomerType>(context), ICustomerTypeRepository
{
    public async Task<IReadOnlyList<CustomerType>> GetAllActiveAsync(
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(ct => ct.IsActive)
            .OrderBy(ct => ct.Name)
            .ToListAsync(cancellationToken);
}

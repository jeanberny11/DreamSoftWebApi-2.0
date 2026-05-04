using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ICustomerStatusRepository : IRepository<CustomerStatus>
{
    Task<IReadOnlyList<CustomerStatus>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}

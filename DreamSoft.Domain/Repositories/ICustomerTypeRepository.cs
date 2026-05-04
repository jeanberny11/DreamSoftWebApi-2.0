using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ICustomerTypeRepository : IRepository<CustomerType>
{
    Task<IReadOnlyList<CustomerType>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}

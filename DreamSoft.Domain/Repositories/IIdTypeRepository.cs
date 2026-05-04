using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IIdTypeRepository : IRepository<IdType>
{
    Task<IReadOnlyList<IdType>> GetByCountryIdAsync(int countryId, CancellationToken cancellationToken = default);
}

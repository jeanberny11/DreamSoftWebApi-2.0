using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IProvinceRepository : IRepository<Province>
{
    Task<IReadOnlyList<Province>> GetByCountryIdAsync(int countryId, CancellationToken cancellationToken = default);
}

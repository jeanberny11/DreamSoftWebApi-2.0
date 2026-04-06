using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IMunicipalityRepository : IRepository<Municipality>
{
    Task<IReadOnlyList<Municipality>> GetByProvinceIdAsync(int provinceId, CancellationToken cancellationToken = default);
}

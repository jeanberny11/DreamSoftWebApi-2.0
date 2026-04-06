using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ICountryRepository : IRepository<Country>
{
    Task<Country?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}

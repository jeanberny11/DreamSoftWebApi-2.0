using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ICurrencyRepository : IRepository<Currency>
{
    Task<Currency?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}

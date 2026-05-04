using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ICurrencyRepository : IRepository<Currency>
{
    Task<Currency?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Currency>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}

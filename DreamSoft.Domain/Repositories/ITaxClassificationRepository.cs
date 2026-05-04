using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ITaxClassificationRepository : IRepository<TaxClassification>
{
    Task<IReadOnlyList<TaxClassification>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<TaxClassification?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}

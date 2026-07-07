using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ISolutionRepository : IRepository<Solution>
{
    Task<Solution?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Solution?> GetByCodeWithPlansAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Solution>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Solution>> GetAllActiveWithModulesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Solution>> GetAllActiveWithPlansAsync(
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Solution>> GetAllActiveWithPlansAndFeaturesAsync(
        CancellationToken cancellationToken = default);
    Task<Solution?> GetActiveByCodeWithPlansAndFeaturesAsync(
        string code, CancellationToken cancellationToken = default);
}

using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ISolutionRepository : IRepository<Solution>
{
    Task<Solution?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Solution>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}

using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ISolutionRepository : IRepository<Solution>
{
    Task<IReadOnlyList<Solution>> GetAllActiveAsync(CancellationToken ct = default);
}

using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IGenderRepository : IRepository<Gender>
{
    Task<IReadOnlyList<Gender>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}

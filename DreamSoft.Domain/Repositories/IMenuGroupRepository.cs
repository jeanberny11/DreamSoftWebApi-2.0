using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IMenuGroupRepository : IRepository<MenuGroup>
{
    Task<IReadOnlyList<MenuGroup>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}

using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IMenuOptionRepository : IRepository<MenuOption>
{
    Task<MenuOption?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}

using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IMenuOptionRepository : IRepository<MenuOption>
{
    Task<IReadOnlyList<MenuOption>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<MenuOption?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MenuOption>> GetAllActiveWithModuleAndGroupAsync(CancellationToken cancellationToken = default);
}

using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IModuleRepository : IRepository<Module>
{
    Task<IReadOnlyList<Module>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Module>> GetAllActiveWithMenuOptionsAsync(CancellationToken cancellationToken = default);
}

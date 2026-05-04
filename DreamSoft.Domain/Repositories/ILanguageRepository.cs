using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ILanguageRepository : IRepository<Language>
{
    Task<Language?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Language?> GetDefaultAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Language>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}

using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Repositories;

public interface IAuditableRepository<T> : IRepository<T> where T : AuditableEntity
{
    Task<IReadOnlyList<T>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}
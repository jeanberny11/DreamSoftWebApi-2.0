using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ITenantStatusRepository : IRepository<TenantStatus>
{
    Task<TenantStatus?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}

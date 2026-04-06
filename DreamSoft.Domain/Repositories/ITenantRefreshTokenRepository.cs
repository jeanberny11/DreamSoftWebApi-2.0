using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ITenantRefreshTokenRepository : IRepository<TenantRefreshToken>
{
    Task<TenantRefreshToken?> GetActiveTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RevokeAllForTenantAsync(int tenantId, string? revokedByIp, CancellationToken cancellationToken = default);
}

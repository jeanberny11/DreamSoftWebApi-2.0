using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IAdminRefreshTokenRepository : IRepository<AdminRefreshToken>
{
    Task<AdminRefreshToken?> GetActiveTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RevokeAllForAdminAsync(int adminUserId, string? revokedByIp, CancellationToken cancellationToken = default);
}

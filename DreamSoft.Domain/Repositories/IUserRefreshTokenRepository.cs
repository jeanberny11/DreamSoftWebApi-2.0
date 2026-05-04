using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IUserRefreshTokenRepository : IRepository<UserRefreshToken>
{
    Task<UserRefreshToken?> GetActiveTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RevokeAllForUserAsync(int userId, string? revokedByIp, CancellationToken cancellationToken = default);
}

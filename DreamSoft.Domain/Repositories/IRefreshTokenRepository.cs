using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetActiveByTokenAsync(string token, CancellationToken ct = default);
    Task<RefreshToken?> GetActiveByTokenWithUserAsync(string token, CancellationToken ct = default);
    Task<IReadOnlyList<RefreshToken>> GetActiveByUserAsync(int userId, CancellationToken ct = default);
}

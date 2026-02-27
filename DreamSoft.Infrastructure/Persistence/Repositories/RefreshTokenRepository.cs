using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository(ApplicationDbContext context)
    : Repository<RefreshToken>(context), IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetActiveByTokenAsync(string token, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(r =>
            r.Token == token &&
            r.RevokedAt == null &&
            r.ExpiresAt > DateTime.UtcNow, ct);

    public async Task<RefreshToken?> GetActiveByTokenWithUserAsync(string token, CancellationToken ct = default)
        => await _dbSet
            .Include(r => r.User)
            .FirstOrDefaultAsync(r =>
                r.Token == token &&
                r.RevokedAt == null &&
                r.ExpiresAt > DateTime.UtcNow, ct);

    public async Task<IReadOnlyList<RefreshToken>> GetActiveByUserAsync(int userId, CancellationToken ct = default)
        => await _dbSet
            .Where(r =>
                r.UserId == userId &&
                r.RevokedAt == null &&
                r.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(ct);
}

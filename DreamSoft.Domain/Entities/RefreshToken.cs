namespace DreamSoft.Domain.Entities;

/// <summary>
/// Represents a refresh token for maintaining user sessions
/// </summary>
public class RefreshToken : BaseEntity<int>
{
    public int UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public string? CreatedByIp { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedByIp { get; private set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public new bool IsActive => !IsRevoked && !IsExpired;

    // Navigation properties
    public User User { get; private set; } = null!;

    // Private constructor for EF Core
    private RefreshToken() { }

    /// <summary>
    /// Creates a new refresh token
    /// </summary>
    public static RefreshToken Create(
        int userId,
        string token,
        DateTime expiresAt,
        string? createdByIp)
    {
        if (userId <= 0)
            throw new ArgumentException("UserId must be greater than zero", nameof(userId));

        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty", nameof(token));

        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Expiration date must be in the future", nameof(expiresAt));

        return new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = createdByIp
        };
    }

    /// <summary>
    /// Revokes the refresh token
    /// </summary>
    public void Revoke(string? revokedByIp)
    {
        if (IsRevoked)
            throw new InvalidOperationException("Token is already revoked");

        RevokedAt = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
    }
}
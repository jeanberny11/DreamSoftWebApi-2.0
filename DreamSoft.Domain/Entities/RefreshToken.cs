using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

/// <summary>
/// Represents a refresh token for maintaining user sessions
/// </summary>
public class RefreshToken : TenantEntity
{
    public int UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public string? CreatedByIp { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedByIp { get; private set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;

    // Note: Hides the base IsActive property - this entity's active state is based on revocation/expiration
    public new bool IsActive => !IsRevoked && !IsExpired;

    // Navigation properties - Note: Tenant is inherited from TenantEntity
    public User User { get; private set; } = null!;

    // Private constructor for EF Core
    private RefreshToken() { }

    /// <summary>
    /// Creates a new refresh token
    /// </summary>
    public static RefreshToken Create(
        int tenantId,
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

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            CreatedByIp = createdByIp
        };

        refreshToken.InitializeTenantEntity(tenantId, userId); // Initialize tenant + audit fields

        return refreshToken;
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

        MarkAsUpdated(); // Note: UpdatedBy remains null for refresh tokens
    }
}
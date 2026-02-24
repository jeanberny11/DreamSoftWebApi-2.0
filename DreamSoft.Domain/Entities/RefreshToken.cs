using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

/// <summary>
/// A persisted refresh token record. One row is created per login, enabling
/// multiple concurrent sessions per user (multi-device support).
/// Tokens are never deleted — they are revoked so the audit trail is preserved.
/// </summary>
public class RefreshToken : AuditableEntity
{
    public int UserId { get; private set; }

    /// <summary>Cryptographically random opaque token string (64-byte Base64).</summary>
    public string Token { get; private set; } = null!;

    /// <summary>When this token expires. After expiry the token is no longer active.</summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>IP address that created this token (login request origin).</summary>
    public string? CreatedByIp { get; private set; }

    /// <summary>Optional User-Agent / device hint for session-list display.</summary>
    public string? DeviceInfo { get; private set; }

    /// <summary>Set when the token is explicitly revoked (logout or rotation).</summary>
    public DateTime? RevokedAt { get; private set; }

    /// <summary>IP address that revoked this token.</summary>
    public string? RevokedByIp { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;

    /// <summary>True only when the token has not been revoked and has not expired.</summary>
    public new bool IsActive => !IsRevoked && !IsExpired;

    // Navigation
    public User User { get; private set; } = null!;

    private RefreshToken() { }

    /// <summary>
    /// Factory method — creates a new active refresh token for a user login.
    /// </summary>
    public static RefreshToken Create(
        int userId,
        string token,
        DateTime expiresAt,
        string? createdByIp,
        string? deviceInfo = null)
    {
        if (userId <= 0)
            throw new ArgumentException("UserId must be greater than zero.", nameof(userId));

        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty.", nameof(token));

        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("ExpiresAt must be in the future.", nameof(expiresAt));

        var rt = new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            CreatedByIp = createdByIp,
            DeviceInfo = deviceInfo
        };

        rt.InitializeAudit();
        return rt;
    }

    /// <summary>
    /// Marks the token as revoked, recording the IP address that triggered the revocation.
    /// Idempotent — calling Revoke on an already-revoked token is a no-op.
    /// </summary>
    public void Revoke(string? revokedByIp)
    {
        if (IsRevoked) return;

        RevokedAt = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
        MarkAsUpdated();
    }
}

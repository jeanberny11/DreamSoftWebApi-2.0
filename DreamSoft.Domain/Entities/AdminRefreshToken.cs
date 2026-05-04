using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

/// <summary>
/// Persisted refresh token for AdminUser login sessions.
/// Mirrors the pattern of TenantRefreshToken and UserRefreshToken.
/// Tokens are never deleted — they are revoked to preserve the audit trail.
/// </summary>
public class AdminRefreshToken : AuditableEntity
{
    public int AdminUserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public string? CreatedByIp { get; private set; }
    public string? DeviceInfo { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedByIp { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsValid => !IsRevoked && !IsExpired;

    // Navigation property
    public AdminUser AdminUser { get; private set; } = null!;

    private AdminRefreshToken() { }

    public static AdminRefreshToken Create(
        int adminUserId,
        string token,
        DateTime expiresAt,
        string? createdByIp,
        string? deviceInfo = null)
    {
        if (adminUserId <= 0)
            throw new ArgumentException("AdminUser ID must be greater than zero", nameof(adminUserId));

        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty", nameof(token));

        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("ExpiresAt must be in the future", nameof(expiresAt));

        var rt = new AdminRefreshToken
        {
            AdminUserId = adminUserId,
            Token = token,
            ExpiresAt = expiresAt,
            CreatedByIp = createdByIp,
            DeviceInfo = deviceInfo
        };

        rt.InitializeAudit();
        return rt;
    }

    /// <summary>
    /// Marks the token as revoked. Idempotent — calling on an already-revoked token is a no-op.
    /// </summary>
    public void Revoke(string? revokedByIp)
    {
        if (IsRevoked) return;

        RevokedAt = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
        MarkAsUpdated();
    }
}

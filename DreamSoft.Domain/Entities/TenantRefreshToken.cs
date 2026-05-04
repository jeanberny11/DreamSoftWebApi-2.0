using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

/// <summary>
/// Persisted refresh token for Tenant account login sessions.
/// One record is created per login, enabling multiple concurrent sessions (multi-device support).
/// Tokens are never deleted — they are revoked so the audit trail is preserved.
/// </summary>
public class TenantRefreshToken : AuditableEntity
{
    public int TenantId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public string? CreatedByIp { get; private set; }
    public string? DeviceInfo { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedByIp { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsValid => !IsRevoked && !IsExpired;

    // Navigation properties
    public Tenant Tenant { get; private set; } = null!;

    private TenantRefreshToken() { }

    public static TenantRefreshToken Create(
        int tenantId,
        string token,
        DateTime expiresAt,
        string? createdByIp,
        string? deviceInfo = null)
    {
        if (tenantId <= 0)
            throw new ArgumentException("Tenant ID must be greater than zero", nameof(tenantId));

        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty", nameof(token));

        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("ExpiresAt must be in the future", nameof(expiresAt));

        var rt = new TenantRefreshToken
        {
            TenantId = tenantId,
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

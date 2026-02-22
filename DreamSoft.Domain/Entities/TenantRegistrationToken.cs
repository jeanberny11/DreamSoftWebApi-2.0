namespace DreamSoft.Domain.Entities;

/// <summary>
/// Stores the server-side state for an email verification OTP session.
/// One record is created per registration attempt and per resend.
/// The plaintext code is NEVER stored — only the PBKDF2 hash.
/// </summary>
public class TenantRegistrationToken : Common.AuditableEntity
{
    public int TenantId { get; private set; }

    /// <summary>PBKDF2 hash of the 6-digit numeric OTP code.</summary>
    public string CodeHash { get; private set; } = null!;

    /// <summary>UTC expiry — 24 hours from creation.</summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>Incremented on each wrong code attempt. Max: appsettings
    /// RateLimit:MaxVerificationAttemptsPerCode (default 5).</summary>
    public int AttemptCount { get; private set; }

    /// <summary>Set to true after successful verification or when
    /// a resend invalidates this token.</summary>
    public bool IsConsumed { get; private set; }

    // Navigation
    public Tenant Tenant { get; private set; } = null!;

    private TenantRegistrationToken() { }

    public static TenantRegistrationToken Create(
        int tenantId, string codeHash, DateTime expiresAt)
    {
        if (tenantId <= 0)
            throw new ArgumentException(
                "Tenant ID must be greater than zero", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(codeHash))
            throw new ArgumentException(
                "Code hash is required", nameof(codeHash));

        var token = new TenantRegistrationToken
        {
            TenantId = tenantId,
            CodeHash = codeHash,
            ExpiresAt = expiresAt,
            AttemptCount = 0,
            IsConsumed = false
        };
        token.InitializeAudit();
        return token;
    }

    public void IncrementAttempt() => AttemptCount++;

    public void Consume() => IsConsumed = true;

    public bool IsExpired() => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Returns true when this token can still be used for verification.
    /// maxAttempts should be read from IConfiguration.
    /// </summary>
    public bool IsValid(int maxAttempts) =>
        !IsConsumed && !IsExpired() && AttemptCount < maxAttempts;
}

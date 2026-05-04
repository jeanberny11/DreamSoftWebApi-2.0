using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

/// <summary>
/// Represents a platform-level administrator who can manage global/shared data
/// (countries, plans, solutions, lookup tables, etc.).
/// AdminUsers are completely isolated from tenant data — they are NOT TenantEntities.
/// Authentication uses a dedicated JWT scheme (SuperAdminScheme).
/// </summary>
public class AdminUser : AuditableEntity
{
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string RoleCode { get; private set; } = null!;
    public DateTime? LastLoginAt { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutUntil { get; private set; }

    // Navigation properties
    public ICollection<AdminRefreshToken> RefreshTokens { get; private set; } = [];

    private AdminUser() { }

    public static AdminUser Create(
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        string roleCode = "SUPER_ADMIN")
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required", nameof(passwordHash));

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        var admin = new AdminUser
        {
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            RoleCode = roleCode.Trim().ToUpperInvariant()
        };

        admin.InitializeAudit();
        return admin;
    }

    public void UpdatePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required", nameof(passwordHash));

        PasswordHash = passwordHash;
        MarkAsUpdated();
    }

    public string GetFullName() => $"{FirstName} {LastName}";

    // ── Lockout ────────────────────────────────────────────────────────────────
    public const int MaxFailedAttempts = 5;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public bool IsLockedOut() => LockoutUntil.HasValue && DateTime.UtcNow < LockoutUntil.Value;

    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= MaxFailedAttempts)
            LockoutUntil = DateTime.UtcNow.Add(LockoutDuration);
        MarkAsUpdated();
    }

    public void ResetFailedLoginAttempts()
    {
        FailedLoginAttempts = 0;
        LockoutUntil = null;
        MarkAsUpdated();
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        MarkAsUpdated();
    }
}

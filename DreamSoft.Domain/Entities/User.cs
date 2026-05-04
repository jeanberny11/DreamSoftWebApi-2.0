using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class User : TenantEntity
{
    // SolutionId is now inherited from TenantEntity
    public string Username { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string? Phone { get; private set; }
    public string? AvatarUrl { get; private set; }
    public int? LanguageId { get; private set; }
    public int? GenderId { get; private set; }
    public bool IsAdmin { get; private set; }
    public int? RoleId { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutUntil { get; private set; }

    // Navigation properties
    public Gender? Gender { get; private set; }
    public Language? Language { get; private set; }
    public Role? Role { get; private set; }
    public ICollection<UserRefreshToken> RefreshTokens { get; private set; } = [];

    private User() { }

    public static User Create(
        int tenantId,
        int solutionId,
        string username,
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        int? languageId = null,
        int? createdBy = null,
        bool isAdmin = false)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required", nameof(username));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required", nameof(passwordHash));

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        var user = new User
        {
            Username = username.Trim().ToLower(),
            Email = email.Trim().ToLower(),
            PasswordHash = passwordHash,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            LanguageId = languageId,
            IsAdmin = isAdmin
        };

        user.InitializeTenantEntity(tenantId, solutionId, createdBy);
        return user;
    }

    public void UpdateProfile(string firstName, string lastName,
        string? phone = null, int? genderId = null, int? updatedBy = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Phone = phone?.Trim();
        GenderId = genderId;
        RecordUpdate(updatedBy);
    }

    public void UpdateEmail(string email, int? updatedBy = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        Email = email.Trim().ToLower();
        RecordUpdate(updatedBy);
    }

    public void UpdatePassword(string passwordHash, int? updatedBy = null)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required", nameof(passwordHash));

        PasswordHash = passwordHash;
        RecordUpdate(updatedBy);
    }

    public void UpdateAvatar(string? avatarUrl, int? updatedBy = null)
    {
        AvatarUrl = avatarUrl?.Trim();
        RecordUpdate(updatedBy);
    }

    public void UpdateLanguage(int languageId, int? updatedBy = null)
    {
        if (languageId <= 0)
            throw new ArgumentException("Language ID must be greater than zero", nameof(languageId));

        LanguageId = languageId;
        RecordUpdate(updatedBy);
    }

    public void AssignRole(int roleId)
    {
        if (roleId <= 0)
            throw new ArgumentException("Role ID must be greater than zero", nameof(roleId));

        RoleId = roleId;
    }

    public void RemoveRole() { RoleId = null; }

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

    public void RecordLogin() { LastLoginAt = DateTime.UtcNow; MarkAsUpdated(); }

    public string GetFullName() => $"{FirstName} {LastName}";
}

using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class User : TenantEntity
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = null!;
    public string? SecondLastName { get; set; }
    public int? GenderId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int? IdTypeId { get; set; }
    public string? IdNumber { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? AvatarUrl { get; set; }
    public int? LanguageId { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsAdmin { get; private set; }
    public DateTime? LastLoginAt { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutUntil { get; set; }

    // Navigation properties
    public Gender? Gender { get; set; }
    public IdType? IdType { get; set; }
    public Language? Language { get; set; }
    public ICollection<UserRole> UserRoles { get; private set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = [];

    // Self-referential navigation properties for audit trail
    public ICollection<User> CreatedUsers { get; private set; } = [];
    public ICollection<User> UpdatedUsers { get; private set; } = [];
    public ICollection<Role> CreatedRoles { get; private set; } = [];
    public ICollection<Role> UpdatedRoles { get; private set; } = [];
    public ICollection<UserRole> AssignedUserRoles { get; private set; } = [];

    private User() { }

    public static User Create(
        int tenantId,
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
            IsEmailVerified = false,
            IsAdmin = isAdmin
        };

        user.InitializeTenantEntity(tenantId, createdBy);
        return user;
    }

    public void UpdateProfile(
        string firstName,
        string lastName,
        string? middleName = null,
        string? secondLastName = null,
        DateTime? dateOfBirth = null,
        int? genderId = null,
        int? updatedBy = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        MiddleName = middleName?.Trim();
        SecondLastName = secondLastName?.Trim();
        DateOfBirth = dateOfBirth;
        GenderId = genderId;

        RecordUpdate(updatedBy);
    }

    public void UpdateIdentification(int? idTypeId, string? idNumber, int? updatedBy = null)
    {
        IdTypeId = idTypeId;
        IdNumber = idNumber?.Trim();
        RecordUpdate(updatedBy);
    }

    public void UpdateContactInfo(string? phone, string? mobile, int? updatedBy = null)
    {
        Phone = phone?.Trim();
        Mobile = mobile?.Trim();
        RecordUpdate(updatedBy);
    }

    public void UpdateEmail(string email, int? updatedBy = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        Email = email.Trim().ToLower();
        IsEmailVerified = false; // Reset verification when email changes
        RecordUpdate(updatedBy);
    }

    public void VerifyEmail(int? updatedBy = null)
    {
        IsEmailVerified = true;
        RecordUpdate(updatedBy);
    }

    public void UpdatePassword(string passwordHash, int? updatedBy = null)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required", nameof(passwordHash));

        PasswordHash = passwordHash;
        RecordUpdate(updatedBy);
    }

    public void UpdateAvatar(string avatarUrl, int? updatedBy = null)
    {
        AvatarUrl = avatarUrl.Trim();
        RecordUpdate(updatedBy);
    }

    public void UpdateLanguage(int languageId, int? updatedBy = null)
    {
        if (languageId <= 0)
            throw new ArgumentException("Language ID must be greater than zero", nameof(languageId));

        LanguageId = languageId;
        RecordUpdate(updatedBy);
    }

    // ── Lockout constants ───────────────────────────────────────────────────
    public const int MaxFailedAttempts = 5;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public bool IsLockedOut()
        => LockoutUntil.HasValue && DateTime.UtcNow < LockoutUntil.Value;

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

    public string GetFullName()
    {
        var names = new List<string> { FirstName };
        
        if (!string.IsNullOrWhiteSpace(MiddleName))
            names.Add(MiddleName);
        
        names.Add(LastName);
        
        if (!string.IsNullOrWhiteSpace(SecondLastName))
            names.Add(SecondLastName);

        return string.Join(" ", names);
    }
}

using DreamSoft.Domain.Common;
using DreamSoft.Domain.Exceptions;

namespace DreamSoft.Domain.Entities;

public class User : TenantEntity
{
    // Identity
    public string Username { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;

    // Profile
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string? Phone { get; private set; }
    public int? GenderId { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public int? IdTypeId { get; private set; }
    public string? IdNumber { get; private set; }
    public string? Address { get; private set; }
    public string? AvatarUrl { get; private set; }

    // Settings
    public int LanguageId { get; private set; }

    // Status
    public bool IsAdmin { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public DateTime? LastPasswordChangeAt { get; private set; }

    // Navigation properties - Note: Tenant is inherited from TenantEntity
    public Language Language { get; private set; } = null!;
    public Gender? Gender { get; private set; }
    public IdType? IdType { get; private set; }
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = [];

    // Private constructor for EF Core
    private User()
    {
    }

    /// <summary>
    /// Creates a new user
    /// </summary>
    public static User Create(
        int tenantId,
        string username,
        string passwordHash,
        string firstName,
        string lastName,
        int languageId,
        string? phone = null,
        bool isAdmin = false,
        int? createdBy = null)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException("Username is required");

        if (username.Length < 3)
            throw new DomainException("Username must be at least 3 characters");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required");

        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required");

        if (languageId <= 0)
            throw new DomainException("Language ID is required");

        var user = new User
        {
            Username = username.ToLower().Trim(),
            PasswordHash = passwordHash,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Phone = phone?.Trim(),
            LanguageId = languageId,
            IsAdmin = isAdmin,
            LastPasswordChangeAt = DateTime.UtcNow
        };

        user.InitializeTenantEntity(tenantId, createdBy); // Initialize tenant + audit fields

        return user;
    }

    /// <summary>
    /// Full name of the user
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Updates user profile
    /// </summary>
    public void UpdateProfile(
        string firstName,
        string lastName,
        string? phone = null,
        int? genderId = null,
        DateTime? dateOfBirth = null,
        int? idTypeId = null,
        string? idNumber = null,
        string? address = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Phone = phone?.Trim();
        GenderId = genderId;
        DateOfBirth = dateOfBirth;
        IdTypeId = idTypeId;
        IdNumber = idNumber?.Trim();
        Address = address?.Trim();

        MarkAsUpdated(); // User table doesn't track updatedBy
    }

    /// <summary>
    /// Updates username
    /// </summary>
    public void UpdateUsername(string newUsername)
    {
        if (string.IsNullOrWhiteSpace(newUsername))
            throw new DomainException("Username is required");

        if (newUsername.Length < 3)
            throw new DomainException("Username must be at least 3 characters");

        Username = newUsername.ToLower().Trim();
        MarkAsUpdated(); // User table doesn't track updatedBy
    }

    /// <summary>
    /// Updates password hash
    /// </summary>
    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException("Password hash is required");

        PasswordHash = newPasswordHash;
        LastPasswordChangeAt = DateTime.UtcNow;
        MarkAsUpdated(); // User table doesn't track updatedBy
    }

    /// <summary>
    /// Updates avatar
    /// </summary>
    public void UpdateAvatar(string? avatarUrl)
    {
        AvatarUrl = avatarUrl?.Trim();
        MarkAsUpdated(); // User table doesn't track updatedBy
    }

    /// <summary>
    /// Updates preferred language
    /// </summary>
    public void UpdateLanguage(int languageId)
    {
        if (languageId <= 0)
            throw new DomainException("Language ID must be valid");

        LanguageId = languageId;
        MarkAsUpdated(); // User table doesn't track updatedBy
    }

    /// <summary>
    /// Records successful login (doesn't update UpdatedAt)
    /// </summary>
    public void RecordSuccessfulLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Promotes user to admin
    /// </summary>
    public void PromoteToAdmin()
    {
        IsAdmin = true;
        MarkAsUpdated(); // User table doesn't track updatedBy
    }

    /// <summary>
    /// Demotes user from admin
    /// </summary>
    public void DemoteFromAdmin()
    {
        IsAdmin = false;
        MarkAsUpdated(); // User table doesn't track updatedBy
    }
}
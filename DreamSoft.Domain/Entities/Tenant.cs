using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class Tenant : AuditableEntity
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string CompanyName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public bool EmailVerified { get; private set; }
    public DateTime? EmailVerifiedAt { get; private set; }
    public string Phone { get; private set; } = "";
    public string Website { get; private set; } = "";
    public string AddressLine1 { get; private set; } = "";
    public string AddressLine2 { get; private set; } = "";
    public int? CountryId { get; private set; }
    public int? ProvinceId { get; private set; }
    public int? MunicipalityId { get; private set; }
    public string PostalCode { get; private set; } = "";
    public int LanguageId { get; private set; }
    public string LogoUrl { get; private set; } = "";
    public int StatusId { get; private set; }
    public string? StripeCustomerId { get; private set; }
    public string? TermsVersion { get; private set; }
    public DateTime? TermsAcceptedAt { get; private set; }
    public string? TermsAcceptedIp { get; private set; }
    public bool OnboardingCompleted { get; private set; }

    // Account lockout
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutUntil { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    // Navigation properties
    public Country? Country { get; private set; }
    public Province? Province { get; private set; }
    public Municipality? Municipality { get; private set; }
    public Language Language { get; private set; } = null!;
    public TenantStatus Status { get; private set; } = null!;
    public ICollection<TenantSubdomain> TenantSubdomains { get; private set; } = [];
    public ICollection<TenantSubscription> TenantSubscriptions { get; private set; } = [];
    public ICollection<TenantRefreshToken> RefreshTokens { get; private set; } = [];
    public ICollection<TenantRegistrationToken> RegistrationTokens { get; private set; } = [];

    private Tenant() { }

    public static Tenant Create(
        string firstName,
        string lastName,
        string companyName,
        string email,
        string passwordHash,
        int statusId,
        string? phone = null,
        string? addressLine1 = null,
        int languageId = 0)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Company name is required", nameof(companyName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required", nameof(passwordHash));

        if (statusId <= 0)
            throw new ArgumentException("Status ID must be greater than zero", nameof(statusId));

        var tenant = new Tenant
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            CompanyName = companyName.Trim(),
            Email = email.Trim().ToLower(),
            PasswordHash = passwordHash,
            LanguageId = languageId,
            StatusId = statusId,
            Phone = phone?.Trim() ?? "",
            AddressLine1 = addressLine1?.Trim() ?? "",
            EmailVerified = false,
            OnboardingCompleted = false
        };

        tenant.InitializeAudit();
        return tenant;
    }

    public void UpdateCompanyName(string companyName)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Company name is required", nameof(companyName));

        CompanyName = companyName.Trim();
        MarkAsUpdated();
    }

    public void VerifyEmail(DateTime verifiedAt)
    {
        EmailVerified = true;
        EmailVerifiedAt = verifiedAt;
        MarkAsUpdated();
    }

    public void UpdateProfile(string firstName, string lastName,
        string? phone = null, string? website = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Phone = phone?.Trim() ?? "";
        Website = website?.Trim() ?? "";
        MarkAsUpdated();
    }

    public void UpdateAddress(
        string addressLine1,
        string? addressLine2 = null,
        string? postalCode = null,
        int? countryId = null,
        int? provinceId = null,
        int? municipalityId = null)
    {
        AddressLine1 = addressLine1?.Trim() ?? "";
        AddressLine2 = addressLine2?.Trim() ?? "";
        PostalCode = postalCode?.Trim() ?? "";
        CountryId = countryId;
        ProvinceId = provinceId;
        MunicipalityId = municipalityId;
        MarkAsUpdated();
    }

    public void UpdatePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required", nameof(passwordHash));

        PasswordHash = passwordHash;
        MarkAsUpdated();
    }

    public void UpdateLanguage(int languageId)
    {
        if (languageId <= 0)
            throw new ArgumentException("Language ID must be greater than zero", nameof(languageId));

        LanguageId = languageId;
        MarkAsUpdated();
    }

    public void UpdateLogo(string logoUrl)
    {
        LogoUrl = logoUrl?.Trim() ?? "";
        MarkAsUpdated();
    }

    public void UpdateStatus(int statusId)
    {
        if (statusId <= 0)
            throw new ArgumentException("Status ID must be greater than zero", nameof(statusId));

        StatusId = statusId;
        MarkAsUpdated();
    }

    public void SetStripeCustomerId(string stripeCustomerId)
    {
        if (string.IsNullOrWhiteSpace(stripeCustomerId))
            throw new ArgumentException("Stripe customer ID is required", nameof(stripeCustomerId));

        StripeCustomerId = stripeCustomerId.Trim();
        MarkAsUpdated();
    }

    public void AcceptTerms(string version, DateTime acceptedAt, string? acceptedIp)
    {
        if (string.IsNullOrWhiteSpace(version))
            throw new ArgumentException("Terms version is required", nameof(version));

        TermsVersion = version.Trim();
        TermsAcceptedAt = acceptedAt;
        TermsAcceptedIp = acceptedIp;
        MarkAsUpdated();
    }

    public void CompleteOnboarding()
    {
        OnboardingCompleted = true;
        MarkAsUpdated();
    }

    public string GetFullName() => $"{FirstName} {LastName}";

    public bool IsLockedOut() =>
        LockoutUntil.HasValue && LockoutUntil.Value > DateTime.UtcNow;

    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= MaxFailedAttempts)
            LockoutUntil = DateTime.UtcNow.Add(LockoutDuration);
        UpdatedAt = DateTime.UtcNow;
    }

    public void ResetFailedLoginAttempts()
    {
        FailedLoginAttempts = 0;
        LockoutUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

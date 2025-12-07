using DreamSoft.Domain.Exceptions;

namespace DreamSoft.Domain.Entities;

public class Tenant : BaseEntity<int>
{
    // Business Identity
    public string TenantNumber { get; private set; } = null!;
    public string CompanyName { get; private set; } = null!;
    public string Subdomain { get; private set; } = null!;

    // Tax/Legal Information
    public string? TaxId { get; private set; }
    public bool TaxIdVerified { get; private set; }
    public DateTime? TaxIdVerifiedAt { get; private set; }
    public Guid? TaxIdVerifiedBy { get; private set; }

    // Contact Information
    public string Email { get; private set; } = null!;
    public bool EmailVerified { get; private set; }
    public DateTime? EmailVerifiedAt { get; private set; }
    public string? Phone { get; private set; }
    public string? Website { get; private set; }

    // Address
    public string? AddressLine1 { get; private set; }
    public string? AddressLine2 { get; private set; }
    public int? CountryId { get; private set; }
    public int? ProvinceId { get; private set; }
    public int? MunicipalityId { get; private set; }
    public string? PostalCode { get; private set; }

    // Settings
    public string? IndustryType { get; private set; }
    public string Timezone { get; private set; } = "America/Santo_Domingo";
    public string Currency { get; private set; } = "DOP";
    public string DefaultLanguage { get; private set; } = "es";

    // Branding
    public string? LogoUrl { get; private set; }

    // Status (Foreign Key to TenantStatus lookup table)
    public int StatusId { get; private set; }
    public TenantStatus Status { get; private set; } = null!;

    // Private constructor for EF Core
    private Tenant()
    {
    }

    /// <summary>
    /// Creates a new tenant (factory method)
    /// </summary>
    public static Tenant Create(
        string tenantNumber,
        string companyName,
        string subdomain,
        string email,
        int trialStatusId,
        string? taxId = null,
        string? phone = null)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(tenantNumber))
            throw new DomainException("Tenant number is required");

        if (string.IsNullOrWhiteSpace(companyName))
            throw new DomainException("Company name is required");

        if (string.IsNullOrWhiteSpace(subdomain))
            throw new DomainException("Subdomain is required");

        if (!IsValidSubdomain(subdomain))
            throw new DomainException("Subdomain must contain only lowercase letters, numbers, and hyphens");

        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required");

        if (!IsValidEmail(email))
            throw new DomainException("Invalid email format");

        var tenant = new Tenant
        {
            TenantNumber = tenantNumber.Trim(),
            CompanyName = companyName.Trim(),
            Subdomain = subdomain.ToLower().Trim(),
            Email = email.ToLower().Trim(),
            TaxId = taxId?.Trim(),
            Phone = phone?.Trim(),
            StatusId = trialStatusId, // Use the lookup table ID
            EmailVerified = false,
            TaxIdVerified = false
        };

        // Raise domain event (we'll implement event handling later)
        // tenant.RaiseDomainEvent(new TenantCreatedEvent(tenant.Id));

        return tenant;
    }

    /// <summary>
    /// Updates company information
    /// </summary>
    public void UpdateCompanyInfo(
        string companyName,
        string? taxId = null,
        string? phone = null,
        string? website = null)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            throw new DomainException("Company name is required");

        CompanyName = companyName.Trim();
        TaxId = taxId?.Trim();
        Phone = phone?.Trim();
        Website = website?.Trim();
    }

    /// <summary>
    /// Updates contact information
    /// </summary>
    public void UpdateContactInfo(string email, string? phone = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required");

        if (!IsValidEmail(email))
            throw new DomainException("Invalid email format");

        // If email changed, reset verification
        if (Email != email.ToLower().Trim())
        {
            EmailVerified = false;
            EmailVerifiedAt = null;
        }

        Email = email.ToLower().Trim();
        Phone = phone?.Trim();
    }

    /// <summary>
    /// Updates address
    /// </summary>
    public void UpdateAddress(
        string? addressLine1,
        string? addressLine2,
        int? countryId,
        int? provinceId,
        int? municipalityId,
        string? postalCode)
    {
        AddressLine1 = addressLine1?.Trim();
        AddressLine2 = addressLine2?.Trim();
        CountryId = countryId;
        ProvinceId = provinceId;
        MunicipalityId = municipalityId;
        PostalCode = postalCode?.Trim();
    }

    /// <summary>
    /// Updates settings
    /// </summary>
    public void UpdateSettings(
        string? industryType = null,
        string? timezone = null,
        string? currency = null,
        string? defaultLanguage = null)
    {
        if (industryType != null)
            IndustryType = industryType.Trim();

        if (!string.IsNullOrWhiteSpace(timezone))
            Timezone = timezone.Trim();

        if (!string.IsNullOrWhiteSpace(currency))
            Currency = currency.Trim().ToUpper();

        if (!string.IsNullOrWhiteSpace(defaultLanguage))
        {
            var lang = defaultLanguage.ToLower().Trim();
            if (lang != "es" && lang != "en")
                throw new DomainException("Supported languages are 'es' and 'en'");
            DefaultLanguage = lang;
        }
    }

    /// <summary>
    /// Updates logo
    /// </summary>
    public void UpdateLogo(string logoUrl)
    {
        if (string.IsNullOrWhiteSpace(logoUrl))
            throw new DomainException("Logo URL cannot be empty");

        LogoUrl = logoUrl.Trim();
    }

    /// <summary>
    /// Verifies email
    /// </summary>
    public void VerifyEmail()
    {
        EmailVerified = true;
        EmailVerifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifies tax ID
    /// </summary>
    public void VerifyTaxId(Guid verifiedByUserId)
    {
        if (string.IsNullOrWhiteSpace(TaxId))
            throw new DomainException("Cannot verify empty tax ID");

        TaxIdVerified = true;
        TaxIdVerifiedAt = DateTime.UtcNow;
        TaxIdVerifiedBy = verifiedByUserId;
    }

    /// <summary>
    /// Changes the tenant status
    /// </summary>
    public void ChangeStatus(int newStatusId)
    {
        if (newStatusId <= 0)
            throw new DomainException("Invalid status ID");

        StatusId = newStatusId;

        // Raise domain event based on new status
        // We'll implement this later when we add domain events
    }

    /// <summary>
    /// Activates the tenant (helper method)
    /// </summary>
    public void Activate(int activeStatusId)
    {
        if (!EmailVerified)
            throw new DomainException("Email must be verified before activation");

        StatusId = activeStatusId;
        // RaiseDomainEvent(new TenantActivatedEvent(Id));
    }

    /// <summary>
    /// Suspends the tenant (helper method)
    /// </summary>
    public void Suspend(int suspendedStatusId)
    {
        StatusId = suspendedStatusId;
        // RaiseDomainEvent(new TenantSuspendedEvent(Id));
    }

    // Private validation helpers
    private static bool IsValidSubdomain(string subdomain)
    {
        if (string.IsNullOrWhiteSpace(subdomain))
            return false;

        // Only lowercase letters, numbers, and hyphens
        // Must start with a letter
        // Must be between 3-50 characters
        return System.Text.RegularExpressions.Regex.IsMatch(
            subdomain,
            @"^[a-z][a-z0-9-]{2,49}$");
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
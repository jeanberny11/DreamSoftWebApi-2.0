using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class Tenant : AuditableEntity
{
    public string CompanyName { get; set; } = null!;
    public string Subdomain { get; set; } = null!;
    public string TaxId { get; set; } = "";
    public bool TaxIdVerified { get; set; }
    public DateTime? TaxIdVerifiedAt { get; set; }
    public int? TaxIdVerifiedBy { get; set; }
    public string Email { get; set; } = null!;
    public bool EmailVerified { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public string Phone { get; set; } = "";
    public string Website { get; set; } = "";
    public string AddressLine1 { get; set; } = "";
    public string AddressLine2 { get; set; } = "";
    public int? CountryId { get; set; }
    public int? ProvinceId { get; set; }
    public int? MunicipalityId { get; set; }
    public string PostalCode { get; set; } = "";
    public int CurrencyId { get; set; }
    public int LanguageId { get; set; }
    public string LogoUrl { get; set; } = "";
    public int StatusId { get; set; }

    // Navigation properties
    public Country? Country { get; set; }
    public Province? Province { get; set; }
    public Municipality? Municipality { get; set; }
    public Language Language { get; set; } = null!;
    public Currency Currency { get; set; } = null!;
    public TenantStatus Status { get; set; } = null!;
    public ICollection<User> Users { get; private set; } = [];
    public ICollection<Role> Roles { get; private set; } = [];
    public ICollection<TenantSubscription> TenantSubscriptions { get; private set; } = [];

    private Tenant() { }

    public static Tenant Create(
        string companyName,
        string subdomain,
        string email,
        int currencyId,
        int languageId,
        int statusId,
        string? taxId = null,
        string? phone = null,
        string? website = null)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Company name is required", nameof(companyName));

        if (string.IsNullOrWhiteSpace(subdomain))
            throw new ArgumentException("Subdomain is required", nameof(subdomain));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        if (currencyId <= 0)
            throw new ArgumentException("Currency ID must be greater than zero", nameof(currencyId));

        if (languageId <= 0)
            throw new ArgumentException("Language ID must be greater than zero", nameof(languageId));

        if (statusId <= 0)
            throw new ArgumentException("Status ID must be greater than zero", nameof(statusId));

        var tenant = new Tenant
        {
            CompanyName = companyName.Trim(),
            Subdomain = subdomain.ToLower().Trim(),
            Email = email.Trim().ToLower(),
            TaxId = taxId?.Trim() ?? "",
            Phone = phone?.Trim() ?? "",
            Website = website?.Trim() ?? "",
            CurrencyId = currencyId,
            LanguageId = languageId,
            StatusId = statusId,
            EmailVerified = false,
            TaxIdVerified = false
        };

        tenant.InitializeAudit();
        return tenant;
    }

    public void UpdateCompanyInfo(string companyName, string? taxId = null)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Company name is required", nameof(companyName));

        CompanyName = companyName.Trim();
        TaxId = taxId?.Trim() ?? "";
        MarkAsUpdated();
    }

    public void UpdateContactInfo(string email, string? phone = null, string? website = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        Email = email.Trim().ToLower();
        Phone = phone?.Trim() ?? "";
        Website = website?.Trim() ?? "";
        EmailVerified = false;
        MarkAsUpdated();
    }

    public void VerifyEmail(DateTime verifiedAt)
    {
        EmailVerified = true;
        EmailVerifiedAt = verifiedAt;
        MarkAsUpdated();
    }

    public void VerifyTaxId(int verifiedByUserId, DateTime verifiedAt)
    {
        if (verifiedByUserId <= 0)
            throw new ArgumentException("Verified by user ID must be greater than zero", nameof(verifiedByUserId));

        TaxIdVerified = true;
        TaxIdVerifiedAt = verifiedAt;
        TaxIdVerifiedBy = verifiedByUserId;
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

    public void UpdatePreferences(int languageId, int currencyId)
    {
        if (languageId <= 0)
            throw new ArgumentException("Language ID must be greater than zero", nameof(languageId));

        if (currencyId <= 0)
            throw new ArgumentException("Currency ID must be greater than zero", nameof(currencyId));

        LanguageId = languageId;
        CurrencyId = currencyId;
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

    public void UpdateSubdomain(string subdomain)
    {
        if (string.IsNullOrWhiteSpace(subdomain))
            throw new ArgumentException("Subdomain is required", nameof(subdomain));

        Subdomain = subdomain.ToLower().Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Transitions the tenant to a new status.
    /// Business rules about valid transitions are enforced in the handler,
    /// not here, keeping the domain simple.
    /// </summary>
    public void TransitionStatus(int newStatusId)
    {
        if (newStatusId <= 0)
            throw new ArgumentException(
                "Status ID must be greater than zero", nameof(newStatusId));

        StatusId = newStatusId;
        MarkAsUpdated();
    }
}

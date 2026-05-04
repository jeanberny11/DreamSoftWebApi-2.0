using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

/// <summary>
/// Tenant + Solution scoped entity representing a customer record.
/// At least one of Email, Phone, or Mobile must be provided (enforced in Create).
/// </summary>
public class Customer : TenantEntity
{
    // ── Identity ───────────────────────────────────────────────────────────────
    public int CustomerTypeId { get; private set; }
    public int CustomerStatusId { get; private set; }

    // ── Personal / Company Info ────────────────────────────────────────────────
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? CompanyName { get; private set; }
    public string? CommercialName { get; private set; }
    public string? ContactPerson { get; private set; }

    // ── Tax Info ───────────────────────────────────────────────────────────────
    public string? TaxId { get; private set; }
    public int? IdTypeId { get; private set; }
    public int? TaxClassificationId { get; private set; }

    // ── Contact Info ───────────────────────────────────────────────────────────
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Mobile { get; private set; }
    public string? Website { get; private set; }

    // ── Address ────────────────────────────────────────────────────────────────
    public string? AddressLine1 { get; private set; }
    public string? AddressLine2 { get; private set; }
    public int CountryId { get; private set; }
    public int ProvinceId { get; private set; }
    public int MunicipalityId { get; private set; }
    public string? PostalCode { get; private set; }

    // ── Commercial ─────────────────────────────────────────────────────────────
    public decimal CreditLimit { get; private set; }
    public string? PaymentTerms { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public int CurrencyId { get; private set; }
    public string? CustomerCategory { get; private set; }

    // ── Notes ──────────────────────────────────────────────────────────────────
    public string? Notes { get; private set; }

    // ── Navigation properties ──────────────────────────────────────────────────
    public Tenant Tenant { get; private set; } = null!;
    public Solution Solution { get; private set; } = null!;
    public CustomerType CustomerType { get; private set; } = null!;
    public CustomerStatus CustomerStatus { get; private set; } = null!;
    public IdType? IdType { get; private set; }
    public TaxClassification? TaxClassification { get; private set; }
    public Currency Currency { get; private set; } = null!;
    public Country Country { get; private set; } = null!;
    public Province Province { get; private set; } = null!;
    public Municipality Municipality { get; private set; } = null!;
    public User? CreatedByUser { get; private set; }
    public User? UpdatedByUser { get; private set; }

    private Customer() { }

    /// <summary>
    /// Creates a new Customer. At least one of email, phone, or mobile is required.
    /// </summary>
    public static Customer Create(
        int tenantId,
        int solutionId,
        int customerTypeId,
        int customerStatusId,
        int countryId,
        int provinceId,
        int municipalityId,
        string? email,
        string? phone,
        string? mobile,
        int currencyId,
        string? firstName = null,
        string? lastName = null,
        string? companyName = null,
        string? commercialName = null,
        string? contactPerson = null,
        string? taxId = null,
        int? idTypeId = null,
        int? taxClassificationId = null,
        string? website = null,
        string? addressLine1 = null,
        string? addressLine2 = null,
        string? postalCode = null,
        decimal creditLimit = 0,
        string? paymentTerms = null,
        decimal discountPercentage = 0,
        string? customerCategory = null,
        string? notes = null,
        int? createdBy = null)
    {
        // ── Contact constraint (mirrors DB check constraint) ───────────────────
        if (string.IsNullOrWhiteSpace(email)
            && string.IsNullOrWhiteSpace(phone)
            && string.IsNullOrWhiteSpace(mobile))
        {
            throw new ArgumentException(
                "At least one contact method is required: Email, Phone, or Mobile.");
        }

        if (customerTypeId <= 0)
            throw new ArgumentException("Customer type is required", nameof(customerTypeId));

        if (customerStatusId <= 0)
            throw new ArgumentException("Customer status is required", nameof(customerStatusId));

        if (countryId <= 0)
            throw new ArgumentException("Country is required", nameof(countryId));

        if (provinceId <= 0)
            throw new ArgumentException("Province is required", nameof(provinceId));

        if (municipalityId <= 0)
            throw new ArgumentException("Municipality is required", nameof(municipalityId));

        if (creditLimit < 0)
            throw new ArgumentException("Credit limit cannot be negative", nameof(creditLimit));

        if (discountPercentage < 0 || discountPercentage > 100)
            throw new ArgumentException("Discount percentage must be between 0 and 100", nameof(discountPercentage));

        var customer = new Customer
        {
            CustomerTypeId = customerTypeId,
            CustomerStatusId = customerStatusId,
            FirstName = firstName?.Trim(),
            LastName = lastName?.Trim(),
            CompanyName = companyName?.Trim(),
            CommercialName = commercialName?.Trim(),
            ContactPerson = contactPerson?.Trim(),
            TaxId = taxId?.Trim(),
            IdTypeId = idTypeId,
            TaxClassificationId = taxClassificationId,
            Email = email?.Trim().ToLower(),
            Phone = phone?.Trim(),
            Mobile = mobile?.Trim(),
            Website = website?.Trim(),
            AddressLine1 = addressLine1?.Trim(),
            AddressLine2 = addressLine2?.Trim(),
            CountryId = countryId,
            ProvinceId = provinceId,
            MunicipalityId = municipalityId,
            PostalCode = postalCode?.Trim(),
            CreditLimit = creditLimit,
            PaymentTerms = paymentTerms?.Trim(),
            DiscountPercentage = discountPercentage,
            CurrencyId = currencyId,
            CustomerCategory = customerCategory?.Trim(),
            Notes = notes?.Trim()
        };

        customer.InitializeTenantEntity(tenantId, solutionId, createdBy);
        return customer;
    }

    // ── Update methods ─────────────────────────────────────────────────────────

    public void UpdateContactInfo(
        string? email,
        string? phone,
        string? mobile,
        string? website = null,
        int? updatedBy = null)
    {
        if (string.IsNullOrWhiteSpace(email)
            && string.IsNullOrWhiteSpace(phone)
            && string.IsNullOrWhiteSpace(mobile))
        {
            throw new ArgumentException(
                "At least one contact method is required: Email, Phone, or Mobile.");
        }

        Email = email?.Trim().ToLower();
        Phone = phone?.Trim();
        Mobile = mobile?.Trim();
        Website = website?.Trim();
        RecordUpdate(updatedBy);
    }

    public void UpdatePersonalInfo(
        string? firstName,
        string? lastName,
        string? companyName,
        string? commercialName,
        string? contactPerson,
        int? updatedBy = null)
    {
        FirstName = firstName?.Trim();
        LastName = lastName?.Trim();
        CompanyName = companyName?.Trim();
        CommercialName = commercialName?.Trim();
        ContactPerson = contactPerson?.Trim();
        RecordUpdate(updatedBy);
    }

    public void UpdateTaxInfo(
        string? taxId,
        int? idTypeId,
        int? taxClassificationId,
        int? updatedBy = null)
    {
        TaxId = taxId?.Trim();
        IdTypeId = idTypeId;
        TaxClassificationId = taxClassificationId;
        RecordUpdate(updatedBy);
    }

    public void UpdateAddress(
        string? addressLine1,
        string? addressLine2,
        int countryId,
        int provinceId,
        int municipalityId,
        string? postalCode,
        int? updatedBy = null)
    {
        if (countryId <= 0)
            throw new ArgumentException("Country is required", nameof(countryId));

        if (provinceId <= 0)
            throw new ArgumentException("Province is required", nameof(provinceId));

        if (municipalityId <= 0)
            throw new ArgumentException("Municipality is required", nameof(municipalityId));

        AddressLine1 = addressLine1?.Trim();
        AddressLine2 = addressLine2?.Trim();
        CountryId = countryId;
        ProvinceId = provinceId;
        MunicipalityId = municipalityId;
        PostalCode = postalCode?.Trim();
        RecordUpdate(updatedBy);
    }

    public void UpdateCommercialTerms(
        decimal creditLimit,
        string? paymentTerms,
        decimal discountPercentage,
        int currencyId,
        string? customerCategory,
        int? updatedBy = null)
    {
        if (creditLimit < 0)
            throw new ArgumentException("Credit limit cannot be negative", nameof(creditLimit));

        if (discountPercentage < 0 || discountPercentage > 100)
            throw new ArgumentException("Discount percentage must be between 0 and 100", nameof(discountPercentage));

        CreditLimit = creditLimit;
        PaymentTerms = paymentTerms?.Trim();
        DiscountPercentage = discountPercentage;
        CurrencyId = currencyId;
        CustomerCategory = customerCategory?.Trim();
        RecordUpdate(updatedBy);
    }

    public void UpdateStatus(int customerStatusId, int? updatedBy = null)
    {
        if (customerStatusId <= 0)
            throw new ArgumentException("Customer status is required", nameof(customerStatusId));

        CustomerStatusId = customerStatusId;
        RecordUpdate(updatedBy);
    }

    public void UpdateNotes(string? notes, int? updatedBy = null)
    {
        Notes = notes?.Trim();
        RecordUpdate(updatedBy);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    public string GetDisplayName()
        => !string.IsNullOrWhiteSpace(CompanyName)
            ? CompanyName
            : $"{FirstName} {LastName}".Trim();
}

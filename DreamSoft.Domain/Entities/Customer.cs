using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class Customer : TenantEntity
{
    public int CustomerTypeId { get; protected set; }
    public int CustomerStatusId { get; protected set; }

    // Individual fields
    public string? FirstName { get; protected set; }
    public string? LastName { get; protected set; }

    // Business fields
    public string? CompanyName { get; protected set; }
    public string? CommercialName { get; protected set; }
    public string? ContactPerson { get; protected set; }

    // Tax & Legal
    public string? TaxId { get; protected set; }
    public int? IdTypeId { get; protected set; }
    public int? TaxClassificationId { get; protected set; }

    // Contact
    public string? Email { get; protected set; }
    public string? Phone { get; protected set; }
    public string? Mobile { get; protected set; }
    public string? Website { get; protected set; }

    // Address
    public string? AddressLine1 { get; protected set; }
    public string? AddressLine2 { get; protected set; }
    public int CountryId { get; protected set; }
    public int ProvinceId { get; protected set; }
    public int MunicipalityId { get; protected set; }
    public string? PostalCode { get; protected set; }

    // Financial
    public decimal CreditLimit { get; protected set; }
    public string? PaymentTerms { get; protected set; }
    public decimal DiscountPercentage { get; protected set; }
    public string Currency { get; protected set; } = "DOP";

    // Misc
    public string? CustomerCategory { get; protected set; }
    public string? Notes { get; protected set; }

    // Navigation properties
    public CustomerType CustomerType { get; private set; } = null!;
    public CustomerStatus Status { get; private set; } = null!;
    public IdType? IdType { get; private set; }
    public TaxClassification? TaxClassification { get; private set; }
    public Country Country { get; private set; } = null!;
    public Province Province { get; private set; } = null!;
    public Municipality Municipality { get; private set; } = null!;

    private Customer() { }

    public static Customer Create(int tenantId, int customerTypeId, int customerStatusId, int countryId, int provinceId, int municipalityId, int? createdBy, string? firstName = null, string? lastName = null, string? companyName = null, string? email = null, string? phone = null)
    {
        if (tenantId <= 0)
            throw new ArgumentException("Tenant ID is required", nameof(tenantId));

        if (customerTypeId <= 0)
            throw new ArgumentException("Customer type ID is required", nameof(customerTypeId));

        if (customerStatusId <= 0)
            throw new ArgumentException("Customer status ID is required", nameof(customerStatusId));

        if (countryId <= 0)
            throw new ArgumentException("Country ID is required", nameof(countryId));

        if (provinceId <= 0)
            throw new ArgumentException("Province ID is required", nameof(provinceId));

        if (municipalityId <= 0)
            throw new ArgumentException("Municipality ID is required", nameof(municipalityId));

        // At least one contact method required
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Email or phone is required");

        var customer = new Customer
        {
            CustomerTypeId = customerTypeId,
            CustomerStatusId = customerStatusId,
            FirstName = firstName?.Trim(),
            LastName = lastName?.Trim(),
            CompanyName = companyName?.Trim(),
            Email = email?.ToLower().Trim(),
            Phone = phone?.Trim(),
            CountryId = countryId,
            ProvinceId = provinceId,
            MunicipalityId = municipalityId,
            CreditLimit = 0,
            DiscountPercentage = 0
        };

        customer.InitializeTenantEntity(tenantId, createdBy);
        return customer;
    }
}

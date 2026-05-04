using MediatR;

namespace DreamSoft.Application.Features.Customers.GetCustomer;

public record GetCustomerQuery(int CustomerId) : IRequest<GetCustomerResponse>;

// ── Nested reference DTOs ─────────────────────────────────────────────────────

public record CustomerTypeRef(int Id, string Name);

public record CustomerStatusRef(int Id, string Name);

public record IdTypeRef(int Id, string Name);

public record TaxClassificationRef(int Id, string Name);

public record CountryRef(int Id, string Name);

public record ProvinceRef(int Id, string Name);

public record MunicipalityRef(int Id, string Name);

public record CurrencyRef(int Id, string Code, string Name);

// ── Response ──────────────────────────────────────────────────────────────────

public record GetCustomerResponse(
    int Id,
    int TenantId,
    int SolutionId,

    // Type / Status
    CustomerTypeRef CustomerType,
    CustomerStatusRef CustomerStatus,

    // Personal / Company
    string? FirstName,
    string? LastName,
    string? CompanyName,
    string? CommercialName,
    string? ContactPerson,

    // Tax
    string? TaxId,
    IdTypeRef? IdType,
    TaxClassificationRef? TaxClassification,

    // Contact
    string? Email,
    string? Phone,
    string? Mobile,
    string? Website,

    // Address
    string? AddressLine1,
    string? AddressLine2,
    CountryRef Country,
    ProvinceRef Province,
    MunicipalityRef Municipality,
    string? PostalCode,

    // Commercial
    decimal CreditLimit,
    string? PaymentTerms,
    decimal DiscountPercentage,
    CurrencyRef Currency,
    string? CustomerCategory,

    // Notes
    string? Notes,

    // Audit
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

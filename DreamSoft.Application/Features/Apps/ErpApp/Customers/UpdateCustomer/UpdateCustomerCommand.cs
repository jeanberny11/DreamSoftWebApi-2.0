using MediatR;

namespace DreamSoft.Application.Features.Apps.ErpApp.Customers.UpdateCustomer;

public record UpdateCustomerCommand(
    int CustomerId,

    // Personal / Company
    string? FirstName,
    string? LastName,
    string? CompanyName,
    string? CommercialName,
    string? ContactPerson,

    // Contact (at least one required)
    string? Email,
    string? Phone,
    string? Mobile,
    string? Website,

    // Tax
    string? TaxId,
    int? IdTypeId,
    int? TaxClassificationId,

    // Address
    string? AddressLine1,
    string? AddressLine2,
    int CountryId,
    int ProvinceId,
    int MunicipalityId,
    string? PostalCode,

    // Commercial
    decimal CreditLimit,
    string? PaymentTerms,
    decimal DiscountPercentage,
    int CurrencyId,
    string? CustomerCategory,

    // Status
    int CustomerStatusId,

    // Notes
    string? Notes
) : IRequest<UpdateCustomerResponse>;

public record UpdateCustomerResponse(int CustomerId);

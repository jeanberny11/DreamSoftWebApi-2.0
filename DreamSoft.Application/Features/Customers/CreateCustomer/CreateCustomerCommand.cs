using MediatR;

namespace DreamSoft.Application.Features.Customers.CreateCustomer;

public record CreateCustomerCommand(
    // Type / Status
    int CustomerTypeId,
    int CustomerStatusId,

    // Required address fields
    int CountryId,
    int ProvinceId,
    int MunicipalityId,
    int CurrencyId,

    // At least one of these must be provided
    string? Email,
    string? Phone,
    string? Mobile,

    // Optional personal / company
    string? FirstName,
    string? LastName,
    string? CompanyName,
    string? CommercialName,
    string? ContactPerson,

    // Optional tax
    string? TaxId,
    int? IdTypeId,
    int? TaxClassificationId,

    // Optional contact
    string? Website,

    // Optional address
    string? AddressLine1,
    string? AddressLine2,
    string? PostalCode,

    // Optional commercial
    decimal CreditLimit,
    string? PaymentTerms,
    decimal DiscountPercentage,
    string? CustomerCategory,

    // Optional notes
    string? Notes
) : IRequest<CreateCustomerResponse>;

public record CreateCustomerResponse(int CustomerId);

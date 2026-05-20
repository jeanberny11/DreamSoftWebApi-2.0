using MediatR;

namespace DreamSoft.Application.Features.Apps.ErpApp.Customers.GetCustomers;

public record GetCustomersQuery : IRequest<GetCustomersResponse>;

public record GetCustomersResponse(IReadOnlyList<CustomerSummaryDto> Customers);

public record CustomerSummaryDto(
    int Id,
    string DisplayName,
    string? TaxId,
    string? Email,
    string? Phone,
    string? Mobile,
    int CustomerTypeId,
    string CustomerTypeName,
    int CustomerStatusId,
    string CustomerStatusName,
    bool IsActive);

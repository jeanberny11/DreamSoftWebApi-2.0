using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Customers.GetCustomer;

public class GetCustomerQueryHandler(
    ICustomerRepository customerRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetCustomerQuery, GetCustomerResponse>
{
    public async Task<GetCustomerResponse> Handle(
        GetCustomerQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.TenantId
            ?? throw new UnauthorizedException();
        var solutionId = currentUserService.SolutionId
            ?? throw new UnauthorizedException();

        var customer = await customerRepository.GetByIdWithDetailsAsync(
            request.CustomerId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "Customer", request.CustomerId);

        if (customer.TenantId != tenantId || customer.SolutionId != solutionId)
            throw new ForbiddenException(ForbiddenErrorCodes.RoleNotAuthorized, ErrorMessageKeys.Forbidden);

        return new GetCustomerResponse(
            customer.Id,
            customer.TenantId,
            customer.SolutionId,

            new CustomerTypeRef(customer.CustomerTypeId, customer.CustomerType.Name),
            new CustomerStatusRef(customer.CustomerStatusId, customer.CustomerStatus.Name),

            customer.FirstName,
            customer.LastName,
            customer.CompanyName,
            customer.CommercialName,
            customer.ContactPerson,

            customer.TaxId,
            customer.IdType is null ? null : new IdTypeRef(customer.IdTypeId!.Value, customer.IdType.Name),
            customer.TaxClassification is null ? null : new TaxClassificationRef(customer.TaxClassificationId!.Value, customer.TaxClassification.Name),

            customer.Email,
            customer.Phone,
            customer.Mobile,
            customer.Website,

            customer.AddressLine1,
            customer.AddressLine2,
            new CountryRef(customer.CountryId, customer.Country.Name),
            new ProvinceRef(customer.ProvinceId, customer.Province.Name),
            new MunicipalityRef(customer.MunicipalityId, customer.Municipality.Name),
            customer.PostalCode,

            customer.CreditLimit,
            customer.PaymentTerms,
            customer.DiscountPercentage,
            new CurrencyRef(customer.CurrencyId, customer.Currency.Code, customer.Currency.Name),
            customer.CustomerCategory,

            customer.Notes,

            customer.IsActive,
            customer.CreatedAt,
            customer.UpdatedAt);
    }
}

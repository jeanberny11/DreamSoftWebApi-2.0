using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Customers.GetCustomers;

public class GetCustomersQueryHandler(
    ICustomerRepository customerRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetCustomersQuery, GetCustomersResponse>
{
    public async Task<GetCustomersResponse> Handle(
        GetCustomersQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.TenantId
            ?? throw new UnauthorizedException();
        var solutionId = currentUserService.SolutionId
            ?? throw new UnauthorizedException();

        var customers = await customerRepository.GetByTenantAndSolutionAsync(
            tenantId, solutionId, cancellationToken);

        var dtos = customers
            .Select(c => new CustomerSummaryDto(
                c.Id,
                c.GetDisplayName(),
                c.TaxId,
                c.Email,
                c.Phone,
                c.Mobile,
                c.CustomerTypeId,
                c.CustomerType.Name,
                c.CustomerStatusId,
                c.CustomerStatus.Name,
                c.IsActive))
            .ToList();

        return new GetCustomersResponse(dtos);
    }
}

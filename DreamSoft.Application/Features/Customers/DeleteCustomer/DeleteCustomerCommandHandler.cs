using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Customers.DeleteCustomer;

public class DeleteCustomerCommandHandler(
    ICustomerRepository customerRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteCustomerCommand>
{
    public async Task Handle(
        DeleteCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.TenantId
            ?? throw new UnauthorizedException();
        var solutionId = currentUserService.SolutionId
            ?? throw new UnauthorizedException();

        var customer = await customerRepository.GetByIdAsync(
            request.CustomerId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "Customer", request.CustomerId);

        if (customer.TenantId != tenantId || customer.SolutionId != solutionId)
            throw new ForbiddenException(ForbiddenErrorCodes.RoleNotAuthorized, ErrorMessageKeys.Forbidden);

        customer.Deactivate();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.ErpApp.Customers.UpdateCustomer;

public class UpdateCustomerCommandHandler(
    ICustomerRepository customerRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCustomerCommand, UpdateCustomerResponse>
{
    public async Task<UpdateCustomerResponse> Handle(
        UpdateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.TenantId
            ?? throw new UnauthorizedException();
        var solutionId = currentUserService.SolutionId
            ?? throw new UnauthorizedException();
        var userId = currentUserService.UserId;

        var customer = await customerRepository.GetByIdWithDetailsAsync(
            request.CustomerId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "Customer", request.CustomerId);

        if (customer.TenantId != tenantId || customer.SolutionId != solutionId)
            throw new ForbiddenException(ForbiddenErrorCodes.RoleNotAuthorized, ErrorMessageKeys.Forbidden);

        // Check for duplicate TaxId if it changed
        if (!string.IsNullOrWhiteSpace(request.TaxId)
            && !string.Equals(request.TaxId.Trim(), customer.TaxId, StringComparison.OrdinalIgnoreCase))
        {
            if (await customerRepository.ExistsByTaxIdAsync(
                    tenantId, solutionId, request.TaxId, cancellationToken))
            {
                throw new ConflictException("CustomerTaxIdAlreadyExists", request.TaxId);
            }
        }

        customer.UpdatePersonalInfo(
            request.FirstName,
            request.LastName,
            request.CompanyName,
            request.CommercialName,
            request.ContactPerson,
            userId);

        customer.UpdateContactInfo(
            request.Email,
            request.Phone,
            request.Mobile,
            request.Website,
            userId);

        customer.UpdateTaxInfo(
            request.TaxId,
            request.IdTypeId,
            request.TaxClassificationId,
            userId);

        customer.UpdateAddress(
            request.AddressLine1,
            request.AddressLine2,
            request.CountryId,
            request.ProvinceId,
            request.MunicipalityId,
            request.PostalCode,
            userId);

        customer.UpdateCommercialTerms(
            request.CreditLimit,
            request.PaymentTerms,
            request.DiscountPercentage,
            request.CurrencyId,
            request.CustomerCategory,
            userId);

        customer.UpdateStatus(request.CustomerStatusId, userId);

        customer.UpdateNotes(request.Notes, userId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateCustomerResponse(customer.Id);
    }
}

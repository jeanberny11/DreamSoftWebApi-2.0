using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.ErpApp.Customers.CreateCustomer;

public class CreateCustomerCommandHandler(
    ICustomerRepository customerRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCustomerCommand, CreateCustomerResponse>
{
    public async Task<CreateCustomerResponse> Handle(
        CreateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.TenantId
            ?? throw new UnauthorizedException();
        var solutionId = currentUserService.SolutionId
            ?? throw new UnauthorizedException();
        var userId = currentUserService.UserId;

        // Check for duplicate TaxId within the same tenant + solution scope
        if (!string.IsNullOrWhiteSpace(request.TaxId))
        {
            if (await customerRepository.ExistsByTaxIdAsync(
                    tenantId, solutionId, request.TaxId, cancellationToken))
            {
                throw new ConflictException("CustomerTaxIdAlreadyExists", request.TaxId);
            }
        }

        var customer = Customer.Create(
            tenantId:            tenantId,
            solutionId:          solutionId,
            customerTypeId:      request.CustomerTypeId,
            customerStatusId:    request.CustomerStatusId,
            countryId:           request.CountryId,
            provinceId:          request.ProvinceId,
            municipalityId:      request.MunicipalityId,
            email:               request.Email,
            phone:               request.Phone,
            mobile:              request.Mobile,
            currencyId:          request.CurrencyId,
            firstName:           request.FirstName,
            lastName:            request.LastName,
            companyName:         request.CompanyName,
            commercialName:      request.CommercialName,
            contactPerson:       request.ContactPerson,
            taxId:               request.TaxId,
            idTypeId:            request.IdTypeId,
            taxClassificationId: request.TaxClassificationId,
            website:             request.Website,
            addressLine1:        request.AddressLine1,
            addressLine2:        request.AddressLine2,
            postalCode:          request.PostalCode,
            creditLimit:         request.CreditLimit,
            paymentTerms:        request.PaymentTerms,
            discountPercentage:  request.DiscountPercentage,
            customerCategory:    request.CustomerCategory,
            notes:               request.Notes,
            createdBy:           userId);

        await customerRepository.AddAsync(customer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateCustomerResponse(customer.Id);
    }
}

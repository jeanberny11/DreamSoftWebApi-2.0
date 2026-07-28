using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.LandingApp.Invoice.Dtos;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Invoice.GetInvoiceByStripeId;

public class GetInvoiceByStripeIdQueryHandler(
    ISubscriptionInvoiceRepository subscriptionInvoiceRepository,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<GetInvoiceByStripeIdQuery, InvoiceDto>
{
    public async Task<InvoiceDto> Handle(
        GetInvoiceByStripeIdQuery request, CancellationToken cancellationToken)
    {
        var language = request.Language ?? "es";
        var tenantId = currentTenantService.TenantId
            ?? throw new UnauthorizedException("Unauthorized");

        var invoice = await subscriptionInvoiceRepository.GetByTenantAndStripeInvoiceIdAsync(
            tenantId, request.StripeInvoiceId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.InvoiceNotFound, request.StripeInvoiceId);

        return InvoiceDtoMapper.ToDto(invoice, language);
    }
}

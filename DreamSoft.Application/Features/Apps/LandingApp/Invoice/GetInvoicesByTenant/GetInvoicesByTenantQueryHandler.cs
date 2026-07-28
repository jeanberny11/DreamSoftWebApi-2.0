using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.LandingApp.Invoice.Dtos;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Invoice.GetInvoicesByTenant;

public class GetInvoicesByTenantQueryHandler(
    ISubscriptionInvoiceRepository subscriptionInvoiceRepository,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<GetInvoicesByTenantQuery, IReadOnlyList<InvoiceDto>>
{
    public async Task<IReadOnlyList<InvoiceDto>> Handle(
        GetInvoicesByTenantQuery request, CancellationToken cancellationToken)
    {
        var language = request.Language ?? "es";
        var tenantId = currentTenantService.TenantId
            ?? throw new UnauthorizedException("Unauthorized");

        var invoices = await subscriptionInvoiceRepository.GetByTenantIdAsync(tenantId, cancellationToken);

        return invoices.Select(i => InvoiceDtoMapper.ToDto(i, language)).ToList();
    }
}

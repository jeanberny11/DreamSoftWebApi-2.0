using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.LandingApp.Invoice.Dtos;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Invoice.GetInvoicesBySolution;

public class GetInvoicesBySolutionQueryHandler(
    ISubscriptionInvoiceRepository subscriptionInvoiceRepository,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<GetInvoicesBySolutionQuery, IReadOnlyList<InvoiceDto>>
{
    public async Task<IReadOnlyList<InvoiceDto>> Handle(
        GetInvoicesBySolutionQuery request, CancellationToken cancellationToken)
    {
        var language = request.Language ?? "es";
        var tenantId = currentTenantService.TenantId
            ?? throw new UnauthorizedException("Unauthorized");

        var invoices = await subscriptionInvoiceRepository.GetByTenantAndSolutionIdAsync(
            tenantId, request.SolutionId, cancellationToken);

        return invoices.Select(i => InvoiceDtoMapper.ToDto(i, language)).ToList();
    }
}

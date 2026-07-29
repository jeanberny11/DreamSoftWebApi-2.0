using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.LandingApp.Invoice.Dtos;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Invoice.GetInvoicesByDateRange;

public class GetInvoicesByDateRangeQueryHandler(
    ISubscriptionInvoiceRepository subscriptionInvoiceRepository,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<GetInvoicesByDateRangeQuery, IReadOnlyList<InvoiceDto>>
{
    public async Task<IReadOnlyList<InvoiceDto>> Handle(
        GetInvoicesByDateRangeQuery request, CancellationToken cancellationToken)
    {
        var language = request.Language ?? "es";
        var tenantId = currentTenantService.TenantId
            ?? throw new UnauthorizedException("Unauthorized");

        // Query-string DateTimes bind with Kind=Unspecified (no offset in the
        // input), but DueDate is a Postgres `timestamptz` column — Npgsql
        // requires a UTC-kind DateTime to write/compare against it.
        var startDateUtc = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc);
        var endDateUtc = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc);

        var invoices = await subscriptionInvoiceRepository.GetByTenantAndDateRangeAsync(
            tenantId, startDateUtc, endDateUtc, cancellationToken);

        return invoices.Select(i => InvoiceDtoMapper.ToDto(i, language)).ToList();
    }
}

using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.LandingApp.Payment.Dtos;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Payment.GetPaymentsByDateRange;

public class GetPaymentsByDateRangeQueryHandler(
    ISubscriptionPaymentRepository subscriptionPaymentRepository,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<GetPaymentsByDateRangeQuery, IReadOnlyList<PaymentHistoryDto>>
{
    public async Task<IReadOnlyList<PaymentHistoryDto>> Handle(
        GetPaymentsByDateRangeQuery request, CancellationToken cancellationToken)
    {
        var language = request.Language ?? "es";
        var tenantId = currentTenantService.TenantId
            ?? throw new UnauthorizedException("Unauthorized");

        // Query-string DateTimes bind with Kind=Unspecified (no offset in the
        // input), but PaymentDate is a Postgres `timestamptz` column — Npgsql
        // requires a UTC-kind DateTime to write/compare against it.
        var startDateUtc = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc);
        var endDateUtc = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc);

        var payments = await subscriptionPaymentRepository.GetByTenantAndDateRangeAsync(
            tenantId, startDateUtc, endDateUtc, cancellationToken);

        return payments.Select(p => PaymentHistoryDtoMapper.ToDto(p, language)).ToList();
    }
}

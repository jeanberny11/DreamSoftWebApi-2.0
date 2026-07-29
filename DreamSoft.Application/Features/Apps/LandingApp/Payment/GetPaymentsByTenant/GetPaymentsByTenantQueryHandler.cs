using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.LandingApp.Payment.Dtos;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Payment.GetPaymentsByTenant;

public class GetPaymentsByTenantQueryHandler(
    ISubscriptionPaymentRepository subscriptionPaymentRepository,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<GetPaymentsByTenantQuery, IReadOnlyList<PaymentHistoryDto>>
{
    public async Task<IReadOnlyList<PaymentHistoryDto>> Handle(
        GetPaymentsByTenantQuery request, CancellationToken cancellationToken)
    {
        var language = request.Language ?? "es";
        var tenantId = currentTenantService.TenantId
            ?? throw new UnauthorizedException("Unauthorized");

        var payments = await subscriptionPaymentRepository.GetByTenantIdWithDetailsAsync(
            tenantId, null, cancellationToken);

        return payments.Select(p => PaymentHistoryDtoMapper.ToDto(p, language)).ToList();
    }
}

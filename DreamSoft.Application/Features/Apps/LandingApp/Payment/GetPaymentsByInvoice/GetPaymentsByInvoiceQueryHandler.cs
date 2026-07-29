using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.LandingApp.Payment.Dtos;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Payment.GetPaymentsByInvoice;

public class GetPaymentsByInvoiceQueryHandler(
    ISubscriptionPaymentRepository subscriptionPaymentRepository,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<GetPaymentsByInvoiceQuery, IReadOnlyList<PaymentHistoryDto>>
{
    public async Task<IReadOnlyList<PaymentHistoryDto>> Handle(
        GetPaymentsByInvoiceQuery request, CancellationToken cancellationToken)
    {
        var language = request.Language ?? "es";
        var tenantId = currentTenantService.TenantId
            ?? throw new UnauthorizedException("Unauthorized");

        var payments = await subscriptionPaymentRepository.GetByTenantAndInvoiceIdAsync(
            tenantId, request.InvoiceId, cancellationToken);

        return payments.Select(p => PaymentHistoryDtoMapper.ToDto(p, language)).ToList();
    }
}

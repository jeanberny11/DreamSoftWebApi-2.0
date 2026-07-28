using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.LandingApp.Payment.Dtos;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Payment.GetPaymentsBySolution;

public class GetPaymentsBySolutionQueryHandler(
    ISubscriptionPaymentRepository subscriptionPaymentRepository,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<GetPaymentsBySolutionQuery, IReadOnlyList<PaymentHistoryDto>>
{
    public async Task<IReadOnlyList<PaymentHistoryDto>> Handle(
        GetPaymentsBySolutionQuery request, CancellationToken cancellationToken)
    {
        var language = request.Language ?? "es";
        var tenantId = currentTenantService.TenantId
            ?? throw new UnauthorizedException("Unauthorized");

        var payments = await subscriptionPaymentRepository.GetByTenantAndSolutionIdAsync(
            tenantId, request.SolutionId, cancellationToken);

        return payments.Select(p => PaymentHistoryDtoMapper.ToDto(p, language)).ToList();
    }
}

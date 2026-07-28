using DreamSoft.Application.Features.Apps.LandingApp.Payment.Dtos;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Payment.GetPaymentsByTenant;

/// <summary>
/// Returns the authenticated tenant's full payment-attempt history
/// (successful and failed) across all subscriptions.
/// </summary>
public record GetPaymentsByTenantQuery(
    string? Language = null
) : IRequest<IReadOnlyList<PaymentHistoryDto>>;

using DreamSoft.Application.Features.Apps.LandingApp.Payment.Dtos;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Payment.GetPaymentsBySolution;

/// <summary>
/// Returns the authenticated tenant's payment-attempt history scoped to a
/// single solution.
/// </summary>
public record GetPaymentsBySolutionQuery(
    int SolutionId,
    string? Language = null
) : IRequest<IReadOnlyList<PaymentHistoryDto>>;

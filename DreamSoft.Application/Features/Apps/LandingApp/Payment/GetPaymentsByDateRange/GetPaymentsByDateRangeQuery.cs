using DreamSoft.Application.Features.Apps.LandingApp.Payment.Dtos;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Payment.GetPaymentsByDateRange;

/// <summary>
/// Returns the authenticated tenant's payment attempts with a PaymentDate
/// falling within the given range (inclusive).
/// </summary>
public record GetPaymentsByDateRangeQuery(
    DateTime StartDate,
    DateTime EndDate,
    string? Language = null
) : IRequest<IReadOnlyList<PaymentHistoryDto>>;

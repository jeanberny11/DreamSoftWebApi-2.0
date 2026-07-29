using DreamSoft.Application.Features.Apps.LandingApp.Payment.Dtos;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Payment.GetPaymentsByInvoice;

/// <summary>
/// Returns the authenticated tenant's payment attempts (successful and
/// failed retries) scoped to a single invoice.
/// </summary>
public record GetPaymentsByInvoiceQuery(
    int InvoiceId,
    string? Language = null
) : IRequest<IReadOnlyList<PaymentHistoryDto>>;

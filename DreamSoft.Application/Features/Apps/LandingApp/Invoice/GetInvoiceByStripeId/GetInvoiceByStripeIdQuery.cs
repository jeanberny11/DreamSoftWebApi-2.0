using DreamSoft.Application.Features.Apps.LandingApp.Invoice.Dtos;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Invoice.GetInvoiceByStripeId;

/// <summary>
/// Returns a single invoice belonging to the authenticated tenant matching
/// a specific Stripe invoice ID.
/// </summary>
public record GetInvoiceByStripeIdQuery(
    string StripeInvoiceId,
    string? Language = null
) : IRequest<InvoiceDto>;

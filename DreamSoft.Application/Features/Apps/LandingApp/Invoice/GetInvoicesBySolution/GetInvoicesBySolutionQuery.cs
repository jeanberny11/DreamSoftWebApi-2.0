using DreamSoft.Application.Features.Apps.LandingApp.Invoice.Dtos;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Invoice.GetInvoicesBySolution;

/// <summary>
/// Returns the authenticated tenant's invoices scoped to a single solution —
/// used by the subscription card's "View Invoices" button.
/// </summary>
public record GetInvoicesBySolutionQuery(
    int SolutionId,
    string? Language = null
) : IRequest<IReadOnlyList<InvoiceDto>>;

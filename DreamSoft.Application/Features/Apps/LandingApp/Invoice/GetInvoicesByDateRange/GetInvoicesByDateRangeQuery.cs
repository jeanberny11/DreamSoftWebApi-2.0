using DreamSoft.Application.Features.Apps.LandingApp.Invoice.Dtos;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Invoice.GetInvoicesByDateRange;

/// <summary>
/// Returns the authenticated tenant's invoices with a DueDate falling within
/// the given range (inclusive).
/// </summary>
public record GetInvoicesByDateRangeQuery(
    DateTime StartDate,
    DateTime EndDate,
    string? Language = null
) : IRequest<IReadOnlyList<InvoiceDto>>;

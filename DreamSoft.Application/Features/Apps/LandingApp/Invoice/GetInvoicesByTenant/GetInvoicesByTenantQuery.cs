using DreamSoft.Application.Features.Apps.LandingApp.Invoice.Dtos;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Invoice.GetInvoicesByTenant;

/// <summary>
/// Returns all of the authenticated tenant's invoices, each with its nested
/// payment attempts (successful and failed), ordered by creation date.
/// </summary>
public record GetInvoicesByTenantQuery(
    string? Language = null
) : IRequest<IReadOnlyList<InvoiceDto>>;

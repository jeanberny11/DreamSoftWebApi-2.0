using DreamSoft.Application.Features.Apps.LandingApp.Invoice.Dtos;
using DreamSoft.Application.Features.Apps.LandingApp.Invoice.GetInvoiceByStripeId;
using DreamSoft.Application.Features.Apps.LandingApp.Invoice.GetInvoicesByDateRange;
using DreamSoft.Application.Features.Apps.LandingApp.Invoice.GetInvoicesBySolution;
using DreamSoft.Application.Features.Apps.LandingApp.Invoice.GetInvoicesByTenant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.LandingApp;

[Authorize(Policy = AuthPolicies.TenantOnly)]
public class InvoiceController : LandingControllerBase
{
    // ── GET /api/v1/landing/invoice ───────────────────────────────────────

    /// <summary>
    /// Returns all of the authenticated tenant's invoices, each with nested
    /// payment attempts.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<InvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetInvoicesByTenantQuery(language), cancellationToken);
        return Ok(result);
    }

    // ── GET /api/v1/landing/invoice/solution/{solutionId} ──────────────────

    /// <summary>
    /// Returns the authenticated tenant's invoices scoped to a single solution.
    /// </summary>
    [HttpGet("solution/{solutionId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<InvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBySolution(
        int solutionId,
        [FromQuery] string? language,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetInvoicesBySolutionQuery(solutionId, language), cancellationToken);
        return Ok(result);
    }

    // ── GET /api/v1/landing/invoice/by-date-range ───────────────────────────

    /// <summary>
    /// Returns the authenticated tenant's invoices with a DueDate falling
    /// within the given range (inclusive).
    /// </summary>
    [HttpGet("by-date-range")]
    [ProducesResponseType(typeof(IReadOnlyList<InvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string? language,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetInvoicesByDateRangeQuery(startDate, endDate, language), cancellationToken);
        return Ok(result);
    }

    // ── GET /api/v1/landing/invoice/by-stripe-id/{stripeInvoiceId} ─────────

    /// <summary>
    /// Returns a single invoice belonging to the authenticated tenant matching
    /// a specific Stripe invoice ID.
    /// </summary>
    [HttpGet("by-stripe-id/{stripeInvoiceId}")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByStripeId(
        string stripeInvoiceId,
        [FromQuery] string? language,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetInvoiceByStripeIdQuery(stripeInvoiceId, language), cancellationToken);
        return Ok(result);
    }
}

using DreamSoft.Application.Features.Apps.LandingApp.Payment.Dtos;
using DreamSoft.Application.Features.Apps.LandingApp.Payment.GetPaymentsByDateRange;
using DreamSoft.Application.Features.Apps.LandingApp.Payment.GetPaymentsByInvoice;
using DreamSoft.Application.Features.Apps.LandingApp.Payment.GetPaymentsBySolution;
using DreamSoft.Application.Features.Apps.LandingApp.Payment.GetPaymentsByTenant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.LandingApp;

[Authorize(Policy = AuthPolicies.TenantOnly)]
public class PaymentController : LandingControllerBase
{
    // ── GET /api/v1/landing/payment ───────────────────────────────────────

    /// <summary>
    /// Returns the authenticated tenant's full payment-attempt history
    /// (successful and failed).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PaymentHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetPaymentsByTenantQuery(language), cancellationToken);
        return Ok(result);
    }

    // ── GET /api/v1/landing/payment/solution/{solutionId} ──────────────────

    /// <summary>
    /// Returns the authenticated tenant's payment-attempt history scoped to
    /// a single solution.
    /// </summary>
    [HttpGet("solution/{solutionId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<PaymentHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBySolution(
        int solutionId,
        [FromQuery] string? language,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetPaymentsBySolutionQuery(solutionId, language), cancellationToken);
        return Ok(result);
    }

    // ── GET /api/v1/landing/payment/by-date-range ───────────────────────────

    /// <summary>
    /// Returns the authenticated tenant's payment attempts with a
    /// PaymentDate falling within the given range (inclusive).
    /// </summary>
    [HttpGet("by-date-range")]
    [ProducesResponseType(typeof(IReadOnlyList<PaymentHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string? language,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetPaymentsByDateRangeQuery(startDate, endDate, language), cancellationToken);
        return Ok(result);
    }

    // ── GET /api/v1/landing/payment/invoice/{invoiceId} ─────────────────────

    /// <summary>
    /// Returns the authenticated tenant's payment attempts (successful and
    /// failed retries) scoped to a single invoice.
    /// </summary>
    [HttpGet("invoice/{invoiceId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<PaymentHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByInvoice(
        int invoiceId,
        [FromQuery] string? language,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetPaymentsByInvoiceQuery(invoiceId, language), cancellationToken);
        return Ok(result);
    }
}

using Asp.Versioning;
using DreamSoft.Application.Features.Apps.AdminApp.PlanPrices.CreatePlanPrice;
using DreamSoft.Application.Features.Apps.AdminApp.PlanPrices.DeletePlanPrice;
using DreamSoft.Application.Features.Apps.AdminApp.PlanPrices.GetPlanPrices;
using DreamSoft.Application.Features.Apps.AdminApp.PlanPrices.UpdatePlanPrice;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.AdminApp;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/subscription-plans/{planId:int}/prices")]
[Authorize(Policy = AuthPolicies.SuperAdminOnly)]
public class PlanPricesController : ControllerBase
{
    private ISender? _mediator;
    private ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    // ── GET /api/v1/admin/subscription-plans/{planId}/prices ─────────────────

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PlanPriceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(int planId, CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetPlanPricesQuery(planId), cancellationToken));

    // ── POST /api/v1/admin/subscription-plans/{planId}/prices ────────────────

    [HttpPost]
    [ProducesResponseType(typeof(PlanPriceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        int planId,
        [FromBody] CreatePlanPriceRequest body,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new CreatePlanPriceCommand(planId, body.BillingCycleId, body.Price),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    // ── PUT /api/v1/admin/subscription-plans/{planId}/prices/{priceId} ───────

    [HttpPut("{priceId:int}")]
    [ProducesResponseType(typeof(PlanPriceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int priceId,
        [FromBody] UpdatePlanPriceRequest body,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(
            new UpdatePlanPriceCommand(priceId, body.Price, body.IsActive),
            cancellationToken));

    // ── DELETE /api/v1/admin/subscription-plans/{planId}/prices/{priceId} ────

    [HttpDelete("{priceId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int priceId, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeletePlanPriceCommand(priceId), cancellationToken);
        return NoContent();
    }
}

// ── Request body DTOs ─────────────────────────────────────────────────────────

public record CreatePlanPriceRequest(int BillingCycleId, decimal Price);
public record UpdatePlanPriceRequest(decimal Price, bool IsActive);

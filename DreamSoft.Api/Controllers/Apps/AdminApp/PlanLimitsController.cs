using Asp.Versioning;
using DreamSoft.Application.Features.Apps.AdminApp.PlanLimits.CreatePlanLimit;
using DreamSoft.Application.Features.Apps.AdminApp.PlanLimits.DeletePlanLimit;
using DreamSoft.Application.Features.Apps.AdminApp.PlanLimits.GetPlanLimits;
using DreamSoft.Application.Features.Apps.AdminApp.PlanLimits.UpdatePlanLimit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.AdminApp;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/subscription-plans/{planId:int}/limits")]
[Authorize(Policy = AuthPolicies.SuperAdminOnly)]
public class PlanLimitsController : ControllerBase
{
    private ISender? _mediator;
    private ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    // ── GET /api/v1/admin/subscription-plans/{planId}/limits ─────────────────

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PlanLimitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(int planId, CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetPlanLimitsQuery(planId), cancellationToken));

    // ── POST /api/v1/admin/subscription-plans/{planId}/limits ────────────────

    [HttpPost]
    [ProducesResponseType(typeof(PlanLimitDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        int planId,
        [FromBody] CreatePlanLimitRequest body,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new CreatePlanLimitCommand(planId, body.LimitKey, body.LimitValue, body.Description),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    // ── PUT /api/v1/admin/subscription-plans/{planId}/limits/{limitId} ───────

    [HttpPut("{limitId:int}")]
    [ProducesResponseType(typeof(PlanLimitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int limitId,
        [FromBody] UpdatePlanLimitRequest body,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(
            new UpdatePlanLimitCommand(limitId, body.LimitValue, body.Description),
            cancellationToken));

    // ── DELETE /api/v1/admin/subscription-plans/{planId}/limits/{limitId} ────

    [HttpDelete("{limitId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int limitId, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeletePlanLimitCommand(limitId), cancellationToken);
        return NoContent();
    }
}

// ── Request body DTOs ─────────────────────────────────────────────────────────

public record CreatePlanLimitRequest(string LimitKey, decimal LimitValue, string? Description);
public record UpdatePlanLimitRequest(decimal LimitValue, string? Description);

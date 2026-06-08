using Asp.Versioning;
using DreamSoft.Application.Features.Apps.AdminApp.PlanMenuOptions.AddPlanMenuOption;
using DreamSoft.Application.Features.Apps.AdminApp.PlanMenuOptions.DTOs;
using DreamSoft.Application.Features.Apps.AdminApp.PlanMenuOptions.GetPlanMenuOptions;
using DreamSoft.Application.Features.Apps.AdminApp.PlanMenuOptions.RemovePlanMenuOption;
using DreamSoft.Application.Features.Apps.AdminApp.PlanMenuOptions.SetPlanMenuOptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.AdminApp;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/subscription-plans/{planId:int}/menu-options")]
[Authorize(Policy = AuthPolicies.SuperAdminOnly)]
public class PlanMenuOptionsController : ControllerBase
{
    private ISender? _mediator;
    private ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    // ── GET /api/v1/admin/subscription-plans/{planId}/menu-options ────────────
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PlanMenuOptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(int planId, CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetPlanMenuOptionsQuery(planId), cancellationToken));

    // ── POST /api/v1/admin/subscription-plans/{planId}/menu-options ───────────
    [HttpPost]
    [ProducesResponseType(typeof(PlanMenuOptionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add(
        int planId,
        [FromBody] AddPlanMenuOptionRequest body,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new AddPlanMenuOptionCommand(planId, body.MenuOptionId),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    // ── PUT /api/v1/admin/subscription-plans/{planId}/menu-options ────────────
    [HttpPut]
    [ProducesResponseType(typeof(IReadOnlyList<PlanMenuOptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Set(
        int planId,
        [FromBody] SetPlanMenuOptionsRequest body,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(
            new SetPlanMenuOptionsCommand(planId, body.MenuOptionIds),
            cancellationToken));

    // ── DELETE /api/v1/admin/subscription-plans/{planId}/menu-options/{menuOptionId}
    [HttpDelete("{menuOptionId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(
        int planId,
        int menuOptionId,
        CancellationToken cancellationToken)
    {
        await Mediator.Send(new RemovePlanMenuOptionCommand(planId, menuOptionId), cancellationToken);
        return NoContent();
    }
}

// ── Request body DTOs ─────────────────────────────────────────────────────────

public record AddPlanMenuOptionRequest(int MenuOptionId);
public record SetPlanMenuOptionsRequest(List<int> MenuOptionIds);

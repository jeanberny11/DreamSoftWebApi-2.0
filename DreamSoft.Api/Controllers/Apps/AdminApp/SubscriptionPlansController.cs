using DreamSoft.Application.Features.Apps.AdminApp.Shared;
using DreamSoft.Application.Features.Apps.AdminApp.SubscriptionPlans.CreateSubscriptionPlan;
using DreamSoft.Application.Features.Apps.AdminApp.SubscriptionPlans.GetSubscriptionPlanById;
using DreamSoft.Application.Features.Apps.AdminApp.SubscriptionPlans.GetSubscriptionPlans;
using DreamSoft.Application.Features.Apps.AdminApp.SubscriptionPlans.UpdateSubscriptionPlan;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.AdminApp;

[Route("api/v{version:apiVersion}/admin/subscription-plans")]
public class SubscriptionPlansController : AdminControllerBase
{
    // ── GET /api/v1/admin/subscription-plans?solutionId=1 ────────────────────
    // SuperAdmin only — all plans (optionally filtered by solution)

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SubscriptionPlanDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? solutionId,
        CancellationToken cancellationToken)
    {
        if (solutionId.HasValue)
            return Ok(await Mediator.Send(
                new GetSubscriptionPlansBySolutionQuery(solutionId.Value), cancellationToken));

        return Ok(await Mediator.Send(new GetSubscriptionPlansQuery(), cancellationToken));
    }

    // ── GET /api/v1/admin/subscription-plans/{id} ─────────────────────────────

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SubscriptionPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetSubscriptionPlanByIdQuery(id), cancellationToken));

    // ── POST /api/v1/admin/subscription-plans ────────────────────────────────

    [HttpPost]
    [ProducesResponseType(typeof(SubscriptionPlanDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSubscriptionPlanCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    // ── PUT /api/v1/admin/subscription-plans/{id} ─────────────────────────────

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(SubscriptionPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateSubscriptionPlanRequest body,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(
            new UpdateSubscriptionPlanCommand(
                id,
                body.Name,
                body.Description,
                body.Translations,
                body.TierLevel,
                body.TrialDays,
                body.IsActive),
            cancellationToken));
}

// ── Request body DTO ──────────────────────────────────────────────────────────

public record UpdateSubscriptionPlanRequest(
    string          Name,
    string?         Description,
    TranslationsDto Translations,
    int             TierLevel,
    int             TrialDays,
    bool            IsActive);

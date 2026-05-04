using DreamSoft.Application.Features.Admin.BillingCycles.CreateBillingCycle;
using DreamSoft.Application.Features.Admin.BillingCycles.GetBillingCycles;
using DreamSoft.Application.Features.Admin.BillingCycles.UpdateBillingCycle;
using DreamSoft.Application.Features.Admin.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Admin;

[Route("api/v{version:apiVersion}/admin/billing-cycles")]
public class BillingCyclesController : AdminControllerBase
{
    // ── GET /api/v1/admin/billing-cycles?language=en ──────────────────────────

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BillingCycleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetBillingCyclesQuery(language), cancellationToken));

    // ── GET /api/v1/admin/billing-cycles/{id}?language=en ────────────────────

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BillingCycleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        int id,
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetBillingCycleByIdQuery(id, language), cancellationToken));

    // ── POST /api/v1/admin/billing-cycles ─────────────────────────────────────

    [HttpPost]
    [ProducesResponseType(typeof(BillingCycleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateBillingCycleCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    // ── PUT /api/v1/admin/billing-cycles/{id} ─────────────────────────────────

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(BillingCycleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateBillingCycleRequest body,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(
            new UpdateBillingCycleCommand(
                id,
                body.Name,
                body.Description,
                body.Translations,
                body.IsActive),
            cancellationToken));
}

// ── Request body DTO ──────────────────────────────────────────────────────────

public record UpdateBillingCycleRequest(
    string          Name,
    string?         Description,
    TranslationsDto Translations,
    bool            IsActive);

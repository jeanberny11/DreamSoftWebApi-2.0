using DreamSoft.Application.Features.Apps.AdminApp.BillingCycles.CreateBillingCycle;
using DreamSoft.Application.Features.Apps.AdminApp.BillingCycles.DTOs;
using DreamSoft.Application.Features.Apps.AdminApp.BillingCycles.GetBillingCycleById;
using DreamSoft.Application.Features.Apps.AdminApp.BillingCycles.GetBillingCycles;
using DreamSoft.Application.Features.Apps.AdminApp.BillingCycles.UpdateBillingCycle;
using DreamSoft.Application.Features.Apps.AdminApp.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.AdminApp;

[Route("api/v{version:apiVersion}/admin/billing-cycles")]
public class BillingCyclesController : AdminControllerBase
{
    // ── GET /api/v1/admin/billing-cycles ──────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BillingCycleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetBillingCyclesQuery(language), cancellationToken));

    // ── GET /api/v1/admin/billing-cycles/active ───────────────────────────────
    [HttpGet("active")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<BillingCycleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllActive(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetActiveBillingCyclesQuery(language), cancellationToken));

    // ── GET /api/v1/admin/billing-cycles/{id} ─────────────────────────────────
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
        return CreatedAtAction(nameof(GetById), new { id = result.Id, version = "1" }, result);
    }

    // ── PUT /api/v1/admin/billing-cycles/{id} ─────────────────────────────────
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateBillingCycleRequest body,
        CancellationToken cancellationToken)
    {
        await Mediator.Send(
            new UpdateBillingCycleCommand(id, body.Name, body.Description, body.Translations, body.IsActive),
            cancellationToken);
        return NoContent();
    }
}

// ── Request body DTO ──────────────────────────────────────────────────────────

public record UpdateBillingCycleRequest(
    string Name,
    string? Description,
    TranslationsDto Translations,
    bool IsActive);

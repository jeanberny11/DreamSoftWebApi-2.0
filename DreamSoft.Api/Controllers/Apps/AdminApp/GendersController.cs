using DreamSoft.Application.Features.Apps.AdminApp.Genders.CreateGender;
using DreamSoft.Application.Features.Apps.AdminApp.Genders.DTOs;
using DreamSoft.Application.Features.Apps.AdminApp.Genders.GetGenderById;
using DreamSoft.Application.Features.Apps.AdminApp.Genders.GetGenders;
using DreamSoft.Application.Features.Apps.AdminApp.Genders.UpdateGender;
using DreamSoft.Application.Features.Apps.AdminApp.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.AdminApp;

public class GendersController : AdminControllerBase
{
    // ── GET /api/v1/admin/genders ─────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<GenderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetGendersQuery(language), cancellationToken));

    // ── GET /api/v1/admin/genders/active ──────────────────────────────────────
    [HttpGet("active")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<GenderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllActive(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetActiveGendersQuery(language), cancellationToken));

    // ── GET /api/v1/admin/genders/{id} ────────────────────────────────────────
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GenderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        int id,
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetGenderByIdQuery(id, language), cancellationToken));

    // ── POST /api/v1/admin/genders ────────────────────────────────────────────
    [HttpPost]
    [ProducesResponseType(typeof(GenderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateGenderCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, version = "1" }, result);
    }

    // ── PUT /api/v1/admin/genders/{id} ────────────────────────────────────────
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateGenderRequest body,
        CancellationToken cancellationToken)
    {
        await Mediator.Send(
            new UpdateGenderCommand(id, body.Name, body.Translations, body.IsActive),
            cancellationToken);
        return NoContent();
    }
}

// ── Request body DTO ──────────────────────────────────────────────────────────

public record UpdateGenderRequest(
    string Name,
    TranslationsDto Translations,
    bool IsActive);

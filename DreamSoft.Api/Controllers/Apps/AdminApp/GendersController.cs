using DreamSoft.Application.Features.Apps.AdminApp.Genders.CreateGender;
using DreamSoft.Application.Features.Apps.AdminApp.Genders.GetGenderById;
using DreamSoft.Application.Features.Apps.AdminApp.Genders.GetGenders;
using DreamSoft.Application.Features.Apps.AdminApp.Genders.UpdateGender;
using DreamSoft.Application.Features.Apps.AdminApp.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.AdminApp;

public class GendersController : AdminControllerBase
{
    // ── GET /api/v1/admin/genders?language=en ─────────────────────────────────
    // SuperAdmin only — returns all records including inactive

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<GenderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetGendersQuery(language), cancellationToken));

    // ── GET /api/v1/admin/genders/active?language=en ──────────────────────────
    // Public — active records only, used by tenants

    [HttpGet("active")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<GenderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllActive(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetActiveGendersQuery(language), cancellationToken));

    // ── GET /api/v1/admin/genders/{id}?language=en ────────────────────────────
    // Public — single record lookup

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
    // SuperAdmin only — inherited from AdminControllerBase

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
        return StatusCode(StatusCodes.Status201Created, result);
    }

    // ── PUT /api/v1/admin/genders/{id} ────────────────────────────────────────
    // SuperAdmin only — inherited from AdminControllerBase

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(GenderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateGenderRequest body,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(
            new UpdateGenderCommand(id, body.Name, body.Translations, body.IsActive),
            cancellationToken));
}

// ── Request body DTO ──────────────────────────────────────────────────────────

public record UpdateGenderRequest(
    string Name,
    TranslationsDto Translations,
    bool IsActive);

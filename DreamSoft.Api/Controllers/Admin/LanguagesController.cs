using DreamSoft.Application.Features.Admin.Languages.CreateLanguage;
using DreamSoft.Application.Features.Admin.Languages.GetLanguageById;
using DreamSoft.Application.Features.Admin.Languages.GetLanguages;
using DreamSoft.Application.Features.Admin.Languages.UpdateLanguage;
using DreamSoft.Application.Features.Admin.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Admin;

public class LanguagesController : AdminControllerBase
{
    // ── GET /api/v1/admin/languages?language=en ───────────────────────────────
    // SuperAdmin only — returns all records including inactive

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<LanguageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetLanguagesQuery(language), cancellationToken));

    // ── GET /api/v1/admin/languages/active?language=en ────────────────────────
    // Public — active records only, used by tenants

    [HttpGet("active")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<LanguageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllActive(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetActiveLanguagesQuery(language), cancellationToken));

    // ── GET /api/v1/admin/languages/{id}?language=en ──────────────────────────
    // Public — single record lookup

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LanguageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        int id,
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetLanguageByIdQuery(id, language), cancellationToken));

    // ── POST /api/v1/admin/languages ──────────────────────────────────────────
    // SuperAdmin only — inherited from AdminControllerBase

    [HttpPost]
    [ProducesResponseType(typeof(LanguageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateLanguageCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    // ── PUT /api/v1/admin/languages/{id} ──────────────────────────────────────
    // SuperAdmin only — inherited from AdminControllerBase

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(LanguageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateLanguageRequest body,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(
            new UpdateLanguageCommand(id, body.Name, body.IsDefault, body.Translations, body.IsActive),
            cancellationToken));
}

// ── Request body DTO ──────────────────────────────────────────────────────────

public record UpdateLanguageRequest(
    string Name,
    bool IsDefault,
    TranslationsDto Translations,
    bool IsActive);

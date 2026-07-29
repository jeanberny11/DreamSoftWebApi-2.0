using DreamSoft.Application.Features.Apps.AdminApp.Languages.CreateLanguage;
using DreamSoft.Application.Features.Apps.AdminApp.Languages.DTOs;
using DreamSoft.Application.Features.Apps.AdminApp.Languages.GetLanguageById;
using DreamSoft.Application.Features.Apps.AdminApp.Languages.GetLanguages;
using DreamSoft.Application.Features.Apps.AdminApp.Languages.UpdateLanguage;
using DreamSoft.Application.Features.Apps.AdminApp.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.AdminApp;

public class LanguagesController : AdminControllerBase
{
    // ── GET /api/v1/admin/languages ───────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<LanguageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetLanguagesQuery(language), cancellationToken));

    // ── GET /api/v1/admin/languages/active ────────────────────────────────────
    [HttpGet("active")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<LanguageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllActive(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetActiveLanguagesQuery(language), cancellationToken));

    // ── GET /api/v1/admin/languages/{id} ──────────────────────────────────────
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
        return CreatedAtAction(nameof(GetById), new { id = result.Id, version = "1" }, result);
    }

    // ── PUT /api/v1/admin/languages/{id} ──────────────────────────────────────
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateLanguageRequest body,
        CancellationToken cancellationToken)
    {
        await Mediator.Send(
            new UpdateLanguageCommand(id, body.Name, body.IsDefault, body.Translations, body.IsActive),
            cancellationToken);
        return NoContent();
    }
}

// ── Request body DTO ──────────────────────────────────────────────────────────

public record UpdateLanguageRequest(
    string Name,
    bool IsDefault,
    TranslationsDto Translations,
    bool IsActive);

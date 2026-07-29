using DreamSoft.Application.Features.Apps.AdminApp.Modules.CreateModule;
using DreamSoft.Application.Features.Apps.AdminApp.Modules.DTOs;
using DreamSoft.Application.Features.Apps.AdminApp.Modules.GetModuleById;
using DreamSoft.Application.Features.Apps.AdminApp.Modules.GetModules;
using DreamSoft.Application.Features.Apps.AdminApp.Modules.UpdateModule;
using DreamSoft.Application.Features.Apps.AdminApp.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.AdminApp;

public class ModulesController : AdminControllerBase
{
    // ── GET /api/v1/admin/modules ─────────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ModuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetModulesQuery(language), cancellationToken));

    // ── GET /api/v1/admin/modules/active ──────────────────────────────────────
    [HttpGet("active")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<ModuleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllActive(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetActiveModulesQuery(language), cancellationToken));

    // ── GET /api/v1/admin/modules/{id} ────────────────────────────────────────
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ModuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        int id,
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetModuleByIdQuery(id, language), cancellationToken));

    // ── POST /api/v1/admin/modules ────────────────────────────────────────────
    [HttpPost]
    [ProducesResponseType(typeof(ModuleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateModuleCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, version = "1" }, result);
    }

    // ── PUT /api/v1/admin/modules/{id} ────────────────────────────────────────
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateModuleRequest body,
        CancellationToken cancellationToken)
    {
        await Mediator.Send(
            new UpdateModuleCommand(
                id, body.Name, body.Description, body.Icon, body.SortOrder, body.Translations, body.IsActive),
            cancellationToken);
        return NoContent();
    }
}

// ── Request body DTO ──────────────────────────────────────────────────────────

public record UpdateModuleRequest(
    string Name,
    string Description,
    string Icon,
    int SortOrder,
    TranslationsDto Translations,
    bool IsActive);

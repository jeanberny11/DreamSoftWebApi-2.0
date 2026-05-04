using DreamSoft.Application.Features.Admin.MenuOptions.CreateMenuOption;
using DreamSoft.Application.Features.Admin.MenuOptions.GetMenuOptionById;
using DreamSoft.Application.Features.Admin.MenuOptions.GetMenuOptions;
using DreamSoft.Application.Features.Admin.MenuOptions.UpdateMenuOption;
using DreamSoft.Application.Features.Admin.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Admin;

[Route("api/v{version:apiVersion}/admin/menu-options")]
public class MenuOptionsController : AdminControllerBase
{
    // ── GET /api/v1/admin/menu-options?language=en ────────────────────────────
    // SuperAdmin only — returns all records including inactive

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MenuOptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetMenuOptionsQuery(language), cancellationToken));

    // ── GET /api/v1/admin/menu-options/active?language=en ─────────────────────
    // Public — active records only

    [HttpGet("active")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<MenuOptionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllActive(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetActiveMenuOptionsQuery(language), cancellationToken));

    // ── GET /api/v1/admin/menu-options/{id}?language=en ──────────────────────
    // Public — single record lookup

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MenuOptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        int id,
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetMenuOptionByIdQuery(id, language), cancellationToken));

    // ── POST /api/v1/admin/menu-options ───────────────────────────────────────
    // SuperAdmin only — inherited from AdminControllerBase

    [HttpPost]
    [ProducesResponseType(typeof(MenuOptionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateMenuOptionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    // ── PUT /api/v1/admin/menu-options/{id} ───────────────────────────────────
    // SuperAdmin only — inherited from AdminControllerBase

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(MenuOptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateMenuOptionRequest body,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(
            new UpdateMenuOptionCommand(
                id,
                body.Name,
                body.Description,
                body.ModuleId,
                body.MenuGroupId,
                body.Route,
                body.Icon,
                body.SortOrder,
                body.Translations,
                body.IsActive),
            cancellationToken));
}

// ── Request body DTO ──────────────────────────────────────────────────────────

public record UpdateMenuOptionRequest(
    string Name,
    string Description,
    int ModuleId,
    int MenuGroupId,
    string Route,
    string Icon,
    int SortOrder,
    TranslationsDto Translations,
    bool IsActive);

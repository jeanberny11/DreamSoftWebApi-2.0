using DreamSoft.Application.Features.Apps.AdminApp.MenuGroups.CreateMenuGroup;
using DreamSoft.Application.Features.Apps.AdminApp.MenuGroups.DTOs;
using DreamSoft.Application.Features.Apps.AdminApp.MenuGroups.GetMenuGroupById;
using DreamSoft.Application.Features.Apps.AdminApp.MenuGroups.GetMenuGroups;
using DreamSoft.Application.Features.Apps.AdminApp.MenuGroups.UpdateMenuGroup;
using DreamSoft.Application.Features.Apps.AdminApp.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.AdminApp;

[Route("api/v{version:apiVersion}/admin/menu-groups")]
public class MenuGroupsController : AdminControllerBase
{
    // ── GET /api/v1/admin/menu-groups ─────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MenuGroupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetMenuGroupsQuery(language), cancellationToken));

    // ── GET /api/v1/admin/menu-groups/active ──────────────────────────────────
    [HttpGet("active")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<MenuGroupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllActive(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetActiveMenuGroupsQuery(language), cancellationToken));

    // ── GET /api/v1/admin/menu-groups/{id} ────────────────────────────────────
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MenuGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        int id,
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetMenuGroupByIdQuery(id, language), cancellationToken));

    // ── POST /api/v1/admin/menu-groups ────────────────────────────────────────
    [HttpPost]
    [ProducesResponseType(typeof(MenuGroupDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateMenuGroupCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, version = "1" }, result);
    }

    // ── PUT /api/v1/admin/menu-groups/{id} ────────────────────────────────────
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateMenuGroupRequest body,
        CancellationToken cancellationToken)
    {
        await Mediator.Send(
            new UpdateMenuGroupCommand(
                id, body.Name, body.Description, body.Icon, body.SortOrder, body.Translations, body.IsActive),
            cancellationToken);
        return NoContent();
    }
}

// ── Request body DTO ──────────────────────────────────────────────────────────

public record UpdateMenuGroupRequest(
    string Name,
    string Description,
    string Icon,
    int SortOrder,
    TranslationsDto Translations,
    bool IsActive);

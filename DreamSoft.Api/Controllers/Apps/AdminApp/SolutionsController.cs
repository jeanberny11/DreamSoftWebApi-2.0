using DreamSoft.Application.Features.Apps.AdminApp.Solutions.CreateSolution;
using DreamSoft.Application.Features.Apps.AdminApp.Solutions.GetSolutionById;
using DreamSoft.Application.Features.Apps.AdminApp.Solutions.GetSolutions;
using DreamSoft.Application.Features.Apps.AdminApp.Solutions.UpdateSolution;
using DreamSoft.Application.Features.Apps.AdminApp.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.AdminApp;

public class SolutionsController : AdminControllerBase
{
    // ── GET /api/v1/admin/solutions?language=en ───────────────────────────────
    // SuperAdmin only — returns all records including inactive

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SolutionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetSolutionsQuery(language), cancellationToken));

    // ── GET /api/v1/admin/solutions/active?language=en ────────────────────────
    // Public — active records only

    [HttpGet("active")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<SolutionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllActive(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetActiveSolutionsQuery(language), cancellationToken));

    // ── GET /api/v1/admin/solutions/{id}?language=en ──────────────────────────
    // Public — single record lookup

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SolutionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        int id,
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetSolutionByIdQuery(id, language), cancellationToken));

    // ── POST /api/v1/admin/solutions ──────────────────────────────────────────
    // SuperAdmin only — inherited from AdminControllerBase

    [HttpPost]
    [ProducesResponseType(typeof(SolutionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSolutionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    // ── PUT /api/v1/admin/solutions/{id} ──────────────────────────────────────
    // SuperAdmin only — inherited from AdminControllerBase

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(SolutionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateSolutionRequest body,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(
            new UpdateSolutionCommand(
                id,
                body.Name,
                body.Description,
                body.Icon,
                body.SortOrder,
                body.Translations,
                body.IsActive),
            cancellationToken));
}

// ── Request body DTO ──────────────────────────────────────────────────────────

public record UpdateSolutionRequest(
    string          Name,
    string?         Description,
    string?         Icon,
    int             SortOrder,
    TranslationsDto Translations,
    bool            IsActive);

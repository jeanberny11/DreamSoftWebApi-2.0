using DreamSoft.Application.Features.Apps.AdminApp.Currencies.CreateCurrency;
using DreamSoft.Application.Features.Apps.AdminApp.Currencies.GetCurrencies;
using DreamSoft.Application.Features.Apps.AdminApp.Currencies.GetCurrencyById;
using DreamSoft.Application.Features.Apps.AdminApp.Currencies.UpdateCurrency;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.AdminApp;

public class CurrenciesController : AdminControllerBase
{
    // ── GET /api/v1/admin/currencies ──────────────────────────────────────────
    // SuperAdmin only — returns all records including inactive

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CurrencyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetCurrenciesQuery(), cancellationToken));

    // ── GET /api/v1/admin/currencies/active ───────────────────────────────────
    // Public — active records only, used by tenants

    [HttpGet("active")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<CurrencyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllActive(CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetActiveCurrenciesQuery(), cancellationToken));

    // ── GET /api/v1/admin/currencies/{id} ────────────────────────────────────
    // Public — single record lookup

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CurrencyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetCurrencyByIdQuery(id), cancellationToken));

    // ── POST /api/v1/admin/currencies ─────────────────────────────────────────
    // SuperAdmin only — inherited from AdminControllerBase

    [HttpPost]
    [ProducesResponseType(typeof(CurrencyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCurrencyCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    // ── PUT /api/v1/admin/currencies/{id} ────────────────────────────────────
    // SuperAdmin only — inherited from AdminControllerBase

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CurrencyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCurrencyRequest body,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(
            new UpdateCurrencyCommand(id, body.Name, body.NativeName, body.IsDefault, body.IsActive),
            cancellationToken));
}

// ── Request body DTO ──────────────────────────────────────────────────────────

public record UpdateCurrencyRequest(
    string Name,
    string NativeName,
    bool IsDefault,
    bool IsActive);

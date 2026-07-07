using DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Dtos;
using DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Solutions.GetAllActiveSolutions;
using DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Solutions.GetSolutionByCode;
using DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Solutions.GetSolutionFeatures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.LandingApp;

[Route("api/v{version:apiVersion}/landing/[controller]")]
public class SolutionsController : LandingControllerBase
{
    [HttpGet("all-active")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<SolutionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetAllActiveSolutions(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetAllActiveSolutionsQuery(language), cancellationToken));

    [HttpGet("with-features")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<SolutionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetSolutionsWithFeatures(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new SolutionQuery(language), cancellationToken));

    [HttpGet("solution-by-code/{code}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SolutionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetSolutionByCode(
        string code,
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetSolutionByCodeQuery(code, language), cancellationToken));
}

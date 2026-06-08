using DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Solutions.GetSolutionByCode;
using DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Solutions.GetSolutionFeatures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.LandingApp;

[Route("api/v{version:apiVersion}/landing/Solutions-Features")]
public class SolutionsController : LandingControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<SolutionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetSolutions(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new SolutionQuery(language), cancellationToken));

    [HttpGet("{code}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GetSolutionByCodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetSolutionByCode(
        string code,
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetSolutionByCodeQuery(code, language), cancellationToken));
}

using DreamSoft.Application.Features.Apps.LandingApp.LandingPage.AppFeatures.GetAllFeatures;
using DreamSoft.Application.Features.Apps.LandingApp.LandingPage.AppFeatures.GetAppFeatures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.LandingApp;

public class FeaturesController : LandingControllerBase
{
    [HttpGet("[action]")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<GetAppFeaturesResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAppFeatures(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetAppFeaturesQuery(language), cancellationToken));

    [HttpGet("[action]")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<Feature>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllFeatures(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetAllFeaturesQuery(language), cancellationToken));
}

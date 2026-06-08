using DreamSoft.Application.Features.Apps.LandingApp.LandingPage.AppFeatures.GetAppFeatures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.AdminApp;

public class AppFeaturesController : AdminControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<GetAppFeaturesResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAppFeatures(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetAppFeaturesQuery(language), cancellationToken));
}

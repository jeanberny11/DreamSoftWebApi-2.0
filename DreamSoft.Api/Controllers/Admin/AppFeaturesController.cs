using DreamSoft.Application.Features.LandingPage.AppFeatures.GetAppFeaturesQuery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Admin;

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

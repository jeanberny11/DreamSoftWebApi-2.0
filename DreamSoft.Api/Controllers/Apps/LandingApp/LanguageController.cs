using DreamSoft.Application.Features.Apps.LandingApp.Language;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.LandingApp;

public class LanguageController : LandingControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<GetLanguageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetLanguages(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetLanguageQuery(language), cancellationToken));
}

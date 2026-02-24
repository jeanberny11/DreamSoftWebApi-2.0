using DreamSoft.Application.Features.LandingPage.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace DreamSoft.Api.Controllers;

[Route("api/v{version:apiVersion}/landing/features")]
public class FeatureController(ILogger<FeatureController> logger) : ApiControllerBase
{
    private readonly ILogger<FeatureController> _logger = logger;

    [HttpGet]
    [ProducesResponseType(typeof(List<FeatureResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetFeatures([FromQuery] string language)
    {
        return Ok(await Mediator.Send(new FeatureRequest { Language = language }));
    }
}
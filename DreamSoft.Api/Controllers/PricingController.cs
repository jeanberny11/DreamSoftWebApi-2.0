using DreamSoft.Application.Features.LandingPage.Pricing;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PricingController(ILogger<PricingController> logger) : ApiControllerBase
{
    private readonly ILogger<PricingController> _logger = logger;

    [HttpGet("[action]")]
    [ProducesResponseType(typeof(List<SolutionDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetSolutions([FromQuery] string language)
    {
        return Ok(await Mediator.Send(new SolutionRequest { Language = language }));
    }

    [HttpGet("[action]")]
    [ProducesResponseType(typeof(List<SubscriptionPlanDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetPlans([FromQuery] string language, [FromQuery] string solutioncode)
    {
        return Ok(await Mediator.Send(new SubscriptionPlanRequest
        {
            Language = language,
            SolutionCode = solutioncode
        }));
    }
}
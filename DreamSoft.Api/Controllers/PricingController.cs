using DreamSoft.Application.Features.LandingPage.Pricing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers;

[Route("api/v{version:apiVersion}/landing/pricing")]
public class PricingController : ApiControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<PricingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetPricing(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new PricingQuery(language), cancellationToken));
}

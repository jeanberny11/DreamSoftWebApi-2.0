using DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Dtos;
using DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Pricing;
using DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Pricing.CheckoutDetail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.LandingApp;

public class PricingController : LandingControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<SolutionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetPricing(
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new PricingQuery(language), cancellationToken));

    [HttpGet("checkout-detail")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CheckoutDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetCheckoutDetail(
        [FromQuery] int planId,
        [FromQuery] string? language,
        CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new CheckoutDetailQuery(planId, language), cancellationToken));
}

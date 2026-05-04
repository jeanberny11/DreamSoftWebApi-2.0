using DreamSoft.Application.Features.Onboarding.CompleteOnboarding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers;

[Authorize(Policy = AuthPolicies.TenantOnly)]
public class OnboardingController : ApiControllerBase
{
    /// <summary>
    /// Completes the onboarding wizard by saving the tenant's company profile,
    /// address, language preference, and terms acceptance.
    /// Sets OnboardingCompleted = true and redirects to /subscribe.
    /// Requires tenant status: PENDING_SUBSCRIPTION with OnboardingCompleted = false.
    /// </summary>
    [HttpPost("complete")]
    [ProducesResponseType(typeof(CompleteOnboardingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Complete(
        [FromBody] CompleteOnboardingCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}

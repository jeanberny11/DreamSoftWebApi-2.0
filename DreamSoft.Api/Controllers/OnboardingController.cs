using DreamSoft.Application.Features.Onboarding.CompleteOnboarding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers;

[Authorize]
public class OnboardingController : ApiControllerBase
{
    /// <summary>Complete the onboarding wizard by selecting a solution
    /// and subscription plan. Transitions tenant to ACTIVE.
    /// Requires a real access token (not the registration token).
    /// </summary>
    [HttpPost("complete")]
    [ProducesResponseType(typeof(CompleteOnboardingResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Complete(
        [FromBody] CompleteOnboardingCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}

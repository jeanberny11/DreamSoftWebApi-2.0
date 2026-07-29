using DreamSoft.Application.Features.Apps.LandingApp.Onboarding.GetOnboardingChecklist;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.LandingApp;

[Authorize(Policy = AuthPolicies.TenantOnly)]
public class OnboardingController : LandingControllerBase
{
    /// <summary>
    /// Returns the current state of the 3 onboarding checklist steps
    /// for the authenticated tenant's dashboard.
    /// </summary>
    [HttpGet("checklist")]
    [ProducesResponseType(typeof(OnboardingChecklistResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChecklist(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetOnboardingChecklistQuery(), cancellationToken);
        return Ok(result);
    }
}

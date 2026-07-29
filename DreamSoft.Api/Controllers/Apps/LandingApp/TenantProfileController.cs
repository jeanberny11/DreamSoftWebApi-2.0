using DreamSoft.Application.Features.Apps.LandingApp.TenantProfile.EditTenantProfile;
using DreamSoft.Application.Features.Apps.LandingApp.TenantProfile.GetTenantProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.LandingApp;

[Authorize(Policy = AuthPolicies.TenantOnly)]
public class TenantProfileController : LandingControllerBase
{
    /// <summary>
    /// Returns the authenticated tenant's profile information.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetTenantProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromQuery] string? language,CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetTenantProfileQuery(language), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates the authenticated tenant's profile (name, company, address, language).
    /// Sets OnboardingCompleted = true if not already done.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Edit(
        [FromBody] EditTenantProfileCommand command,
        CancellationToken cancellationToken)
    {
        await Mediator.Send(command, cancellationToken);
        return NoContent();
    }
}

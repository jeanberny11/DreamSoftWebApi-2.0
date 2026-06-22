using DreamSoft.Application.Features.Apps.LandingApp.VerifyEmail;
using DreamSoft.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.LandingApp;

public class VerifyEmailController : LandingControllerBase
{
    /// <summary>
    /// Verify the email OTP. Tenant is identified from the Bearer token.
    /// Submits the 6-digit code received by email to complete verification.
    /// Requires a valid tenant access token.
    /// </summary>
    [Authorize(Policy = AuthPolicies.TenantOnly)]
    [HttpPost]
    [ProducesResponseType(typeof(VerifyEmailResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(429)]
    public async Task<IActionResult> VerifyEmail(
        [FromBody] VerifyEmailCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}

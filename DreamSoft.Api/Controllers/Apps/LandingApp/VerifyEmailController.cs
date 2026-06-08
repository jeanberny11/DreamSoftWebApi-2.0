using DreamSoft.Application.Features.Apps.LandingApp.VerifyEmail;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.LandingApp;

public class VerifyEmailController : LandingControllerBase
{
    /// <summary>
    /// Verify the email OTP. Pass the email used during registration and the
    /// 6-digit code received by email. No Authorization header required.
    /// Returns access + refresh tokens on success.
    /// </summary>
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

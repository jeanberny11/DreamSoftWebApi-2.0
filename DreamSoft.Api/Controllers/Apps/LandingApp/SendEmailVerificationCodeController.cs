using DreamSoft.Application.Features.Apps.LandingApp.SendEmailVerificationCode;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.LandingApp;

public class SendEmailVerificationCodeController : LandingControllerBase
{
    /// <summary>
    /// Send a new email verification code. Tenant is identified from the Bearer token.
    /// Rate-limited to once every 2 minutes per tenant and 5 times per hour per IP.
    /// Requires a valid Tenant access token.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(409)]
    [ProducesResponseType(429)]
    public async Task<IActionResult> SendEmailVerificationCode(CancellationToken cancellationToken)
    {
        await Mediator.Send(new SendEmailVerificationCodeCommand(), cancellationToken);
        return NoContent();
    }
}

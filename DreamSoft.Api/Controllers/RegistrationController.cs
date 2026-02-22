using DreamSoft.Application.Features.Registration.RegisterTenant;
using DreamSoft.Application.Features.Registration.ResendVerification;
using DreamSoft.Application.Features.Registration.VerifyEmail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers;

[AllowAnonymous]
public class RegistrationController : ApiControllerBase
{
    /// <summary>Register a new tenant. Returns a short-lived registration
    /// token to be used in the verify-email call.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(RegisterTenantResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterTenantCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return StatusCode(201, result);
    }

    /// <summary>Verify the email OTP. Requires the registration token
    /// in the Authorization: Bearer header. Returns access + refresh tokens.
    /// </summary>
    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(VerifyEmailResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> VerifyEmail(
        [FromBody] VerifyEmailRequest body,
        CancellationToken cancellationToken)
    {
        var registrationToken = ExtractBearerToken();
        if (registrationToken is null)
            return Unauthorized();

        var command = new VerifyEmailCommand(registrationToken, body.Code);
        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>Resend the verification code. Rate-limited to once every
    /// 2 minutes. Requires the registration token in Authorization header.
    /// </summary>
    [HttpPost("resend-verification")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(429)]
    public async Task<IActionResult> ResendVerification(
        CancellationToken cancellationToken)
    {
        var registrationToken = ExtractBearerToken();
        if (registrationToken is null)
            return Unauthorized();

        await Mediator.Send(
            new ResendVerificationCommand(registrationToken),
            cancellationToken);
        return NoContent();
    }

    // ── Helper ─────────────────────────────────────────────────────────────
    private string? ExtractBearerToken()
    {
        var auth = Request.Headers.Authorization.FirstOrDefault();
        if (auth is null || !auth.StartsWith("Bearer ",
                StringComparison.OrdinalIgnoreCase))
            return null;
        return auth["Bearer ".Length..].Trim();
    }
}

// Separate request DTO for the body (Code only)
public record VerifyEmailRequest(string Code);

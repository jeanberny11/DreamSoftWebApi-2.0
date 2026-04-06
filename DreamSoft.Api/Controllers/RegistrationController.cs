using DreamSoft.Application.Features.Registration.CheckOnboardingStatus;
using DreamSoft.Application.Features.Registration.CheckSubdomainAvailability;
using DreamSoft.Application.Features.Registration.RegisterTenant;
using DreamSoft.Application.Features.Registration.ResendVerification;
using DreamSoft.Application.Features.Registration.VerifyEmail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers;

[AllowAnonymous]
public class RegistrationController : ApiControllerBase
{
    /// <summary>
    /// Check whether a subdomain is available for registration.
    /// Returns availability status and the normalized subdomain.
    /// Rate-limited at the infrastructure level.
    /// </summary>
    [HttpGet("check-subdomain")]
    [ProducesResponseType(typeof(SubdomainAvailabilityResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CheckSubdomain(
        [FromQuery] string subdomain,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(subdomain))
            return BadRequest("Subdomain is required.");

        var result = await Mediator.Send(
            new CheckSubdomainAvailabilityQuery(subdomain), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Register a new tenant. Returns the admin email and subdomain to be used
    /// in the verify-email call. No registration token is issued.
    /// </summary>
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

    /// <summary>
    /// Verify the email OTP. Pass the email used during registration and the
    /// 6-digit code received by email. No Authorization header required.
    /// Returns access + refresh tokens on success.
    /// </summary>
    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(VerifyEmailResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(409)]
    [ProducesResponseType(429)]
    public async Task<IActionResult> VerifyEmail(
        [FromBody] VerifyEmailRequest body,
        CancellationToken cancellationToken)
    {
        var command = new VerifyEmailCommand(body.Email, body.Code);
        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Check whether a tenant has completed onboarding.
    /// </summary>
    [HttpGet("check-onboarding-status")]
    [ProducesResponseType(typeof(OnboardingStatusResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CheckOnboardingStatus(
        [FromQuery] int tenantId,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new CheckOnboardingStatusQuery(tenantId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Resend the verification code. Pass the email used during registration.
    /// Rate-limited to once every 2 minutes per tenant and 5 times per hour per IP.
    /// No Authorization header required.
    /// </summary>
    [HttpPost("resend-verification")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(409)]
    [ProducesResponseType(429)]
    public async Task<IActionResult> ResendVerification(
        [FromBody] ResendVerificationRequest body,
        CancellationToken cancellationToken)
    {
        await Mediator.Send(
            new ResendVerificationCommand(body.Email),
            cancellationToken);
        return NoContent();
    }
}

// ── Request DTOs ────────────────────────────────────────────────────────────

/// <summary>Body for POST /registration/verify-email</summary>
public record VerifyEmailRequest(string Email, string Code);

/// <summary>Body for POST /registration/resend-verification</summary>
public record ResendVerificationRequest(string Email);

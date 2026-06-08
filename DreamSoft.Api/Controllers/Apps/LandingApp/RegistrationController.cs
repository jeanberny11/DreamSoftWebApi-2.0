using DreamSoft.Application.Features.Apps.LandingApp.Registration.CheckOnboardingStatus;
using DreamSoft.Application.Features.Apps.LandingApp.Registration.CheckSubdomainAvailability;
using DreamSoft.Application.Features.Apps.LandingApp.Registration.RegisterTenant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.LandingApp;

public class RegistrationController : LandingControllerBase
{
    /// <summary>
    /// Check whether a subdomain is available for registration.
    /// Returns availability status and the normalized subdomain.
    /// Rate-limited at the infrastructure level.
    /// </summary>
    [AllowAnonymous]
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
    /// Register a new tenant. Issues an access token and refresh token cookie
    /// immediately so the client is authenticated without a separate login call.
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(RegisterTenantClientResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterTenantCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        SetRefreshTokenCookie(result.RefreshToken);
        return StatusCode(201, RegisterTenantClientResponse.From(result));
    }

    private void SetRefreshTokenCookie(string token)
    {
        Response.Cookies.Append("__Host-tenant_refresh_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure   = true,
            SameSite = SameSiteMode.Strict,
            Path     = "/"
        });
    }

    /// <summary>
    /// Check whether a tenant has completed onboarding.
    /// </summary>
    [AllowAnonymous]
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

}

public record RegisterTenantClientResponse(
    string AccessToken,
    DateTime ExpiresAt,
    int TenantId,
    string Email,
     string FirstName,
    string LastName,
    string LogoUrl,
    string TenantStatus)
{
    public static RegisterTenantClientResponse From(RegisterTenantResponse r) =>
        new(r.AccessToken, r.ExpiresAt, r.TenantId, r.Email, r.FirstName, r.LastName, r.LogoUrl, r.TenantStatus);
}

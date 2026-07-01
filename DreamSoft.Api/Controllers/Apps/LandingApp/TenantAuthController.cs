using DreamSoft.Application.Features.Apps.LandingApp.Registration.RegisterTenant;
using DreamSoft.Application.Features.Apps.LandingApp.TenantAuth.Dtos;
using DreamSoft.Application.Features.Apps.LandingApp.TenantAuth.LoginTenant;
using DreamSoft.Application.Features.Apps.LandingApp.TenantAuth.LogoutTenant;
using DreamSoft.Application.Features.Apps.LandingApp.TenantAuth.RefreshTenantToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.LandingApp;

[Route("api/v{version:apiVersion}/landing/tenant-auth")]
public class TenantAuthController : ApiControllerBase
{
    private const string RefreshTokenCookieName = "tenant_refresh_token";

    /// <summary>
    /// Authenticates a tenant account owner with email and password.
    /// Sets the refresh token as an HTTP-only, Secure, SameSite=Strict cookie.
    /// Returns only the access token and tenant info in the JSON body.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(TenantAuthClientResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Login(
        [FromBody] LoginTenantCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        SetRefreshTokenCookie(result.RefreshToken, command.RememberMe);
        return Ok(TenantAuthClientResponse.From(result));
    }

    /// <summary>
    /// Rotates a tenant refresh token.
    /// Reads the refresh token from the HTTP-only cookie.
    /// Returns a new access token in the body; sets a new refresh-token cookie.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(TenantAuthClientResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var rawToken = Request.Cookies[RefreshTokenCookieName];
        if (string.IsNullOrWhiteSpace(rawToken))
            return Unauthorized();

        var result = await Mediator.Send(new RefreshTenantTokenCommand(rawToken), cancellationToken);
        SetRefreshTokenCookie(result.RefreshToken, persistent: false);
        return Ok(TenantAuthClientResponse.From(result));
    }

    /// <summary>
    /// Logs out the currently authenticated tenant account owner.
    /// Revokes the current session refresh token and clears the HTTP-only cookie.
    /// Requires a valid Tenant Bearer token.
    /// </summary>
    [Authorize(Policy = AuthPolicies.TenantOnly)]
    [HttpPost("logout")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var rawToken = Request.Cookies[RefreshTokenCookieName];
        await Mediator.Send(new LogoutTenantCommand(rawToken), cancellationToken);

        Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            Secure   = true,
            HttpOnly = true,
            SameSite = SameSiteMode.None,
            Path     = "/"
        });

        return NoContent();
    }

    
    /// <summary>
    /// Register a new tenant. Issues an access token and refresh token cookie
    /// immediately so the client is authenticated without a separate login call.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(TenantAuthClientResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterTenantCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        SetRefreshTokenCookie(result.RefreshToken, persistent: false);
        return StatusCode(201, TenantAuthClientResponse.From(result));
    }

    // ── Cookie helpers ───────────────────────────────────────────────────────

    private void SetRefreshTokenCookie(string token, bool persistent)
    {
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure   = true,
            SameSite = SameSiteMode.None,
            Path     = "/"
        };

        if (persistent)
            options.Expires = DateTimeOffset.UtcNow.AddDays(30);

        Response.Cookies.Append(RefreshTokenCookieName, token, options);
    }
}

// ── DTOs ─────────────────────────────────────────────────────────────────────

public record TenantAuthClientResponse(
    string AccessToken,
    DateTime ExpiresAt,
    int TenantId,
    string Email,
    string FirstName,
    string LastName,
    string LogoUrl,
    bool OnboardingCompleted,
    bool EmailVerified,
    string TenantStatusCode)
{
    public static TenantAuthClientResponse From(TenantAuthResponse r) =>
        new(r.AccessToken, r.ExpiresAt, r.TenantId, r.Email, r.FirstName, r.LastName, r.LogoUrl, r.OnboardingCompleted, r.EmailVerified, r.TenantStatusCode);
}
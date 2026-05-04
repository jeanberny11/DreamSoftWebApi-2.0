using DreamSoft.Application.Features.Auth.LoginTenant;
using DreamSoft.Application.Features.Auth.LogoutTenant;
using DreamSoft.Application.Features.Auth.RefreshTenantToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers;

[Route("api/v{version:apiVersion}/tenant-auth")]
public class TenantAuthController : ApiControllerBase
{
    private const string RefreshTokenCookieName = "__Host-tenant_refresh_token";

    /// <summary>
    /// Authenticates a tenant account owner with email and password.
    /// Sets the refresh token as an HTTP-only, Secure, SameSite=Strict cookie.
    /// Returns only the access token and tenant info in the JSON body.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginTenantClientResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Login(
        [FromBody] LoginTenantCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        SetRefreshTokenCookie(result.RefreshToken, command.RememberMe);
        return Ok(LoginTenantClientResponse.From(result));
    }

    /// <summary>
    /// Rotates a tenant refresh token.
    /// Reads the refresh token from the HTTP-only cookie.
    /// Returns a new access token in the body; sets a new refresh-token cookie.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginTenantClientResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var rawToken = Request.Cookies[RefreshTokenCookieName];
        if (string.IsNullOrWhiteSpace(rawToken))
            return Unauthorized();

        var result = await Mediator.Send(new RefreshTenantTokenCommand(rawToken), cancellationToken);
        SetRefreshTokenCookie(result.RefreshToken, persistent: false);
        return Ok(LoginTenantClientResponse.From(result));
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
            SameSite = SameSiteMode.Strict,
            Path     = "/"
        });

        return NoContent();
    }

    // ── Cookie helpers ───────────────────────────────────────────────────────

    private void SetRefreshTokenCookie(string token, bool persistent)
    {
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure   = true,
            SameSite = SameSiteMode.Strict,
            Path     = "/"
        };

        if (persistent)
            options.Expires = DateTimeOffset.UtcNow.AddDays(30);

        Response.Cookies.Append(RefreshTokenCookieName, token, options);
    }
}

// ── DTOs ─────────────────────────────────────────────────────────────────────

public record LoginTenantClientResponse(
    string AccessToken,
    DateTime ExpiresAt,
    int TenantId,
    string CompanyName,
    string Email,
    bool OnboardingCompleted)
{
    public static LoginTenantClientResponse From(LoginTenantResponse r) =>
        new(r.AccessToken, r.ExpiresAt, r.TenantId, r.CompanyName, r.Email, r.OnboardingCompleted);
}

using DreamSoft.Application.Features.Auth;
using DreamSoft.Application.Features.Auth.LoginBySubdomain;
using DreamSoft.Application.Features.Auth.LoginByTenantEmail;
using DreamSoft.Application.Features.Auth.Logout;
using DreamSoft.Application.Features.Auth.RefreshToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers;

public class AuthController : ApiControllerBase
{
    // Cookie name — __Host- prefix enforces Secure + no Domain + Path=/ in browsers.
    private const string RefreshTokenCookieName = "__Host-refresh_token";

    /// <summary>
    /// Login via subdomain routing.
    /// The tenant is resolved automatically from the request subdomain (Host header).
    /// Sets the refresh token as an HTTP-only, Secure, SameSite=Strict cookie.
    /// Returns only the access token and user info in the JSON body.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginClientResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login(
        [FromBody] LoginBySubdomainCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        SetRefreshTokenCookie(result.RefreshToken, command.RememberMe);
        return Ok(LoginClientResponse.From(result));
    }

    /// <summary>
    /// Login via tenant company email.
    /// The tenant is resolved by matching the provided email against Tenant.Email.
    /// Intended for mobile clients or apps that don't use subdomain-based routing.
    /// Sets the refresh token as an HTTP-only cookie.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login-by-email")]
    [ProducesResponseType(typeof(LoginClientResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> LoginByEmail(
        [FromBody] LoginByTenantEmailCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        SetRefreshTokenCookie(result.RefreshToken, command.RememberMe);
        return Ok(LoginClientResponse.From(result));
    }

    /// <summary>
    /// Rotates a refresh token.
    /// Reads the refresh token from the HTTP-only cookie (preferred) or the JSON body (fallback).
    /// Returns a new access token in the body; sets a new refresh-token cookie.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginClientResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Refresh(
        CancellationToken cancellationToken)
    {
        // Prefer the HTTP-only cookie; fall back to body for mobile clients
        var rawToken = Request.Cookies[RefreshTokenCookieName];
        if (string.IsNullOrWhiteSpace(rawToken))
            return Unauthorized();

        var result = await Mediator.Send(new RefreshTokenCommand(rawToken), cancellationToken);

        // Rotate the cookie too
        SetRefreshTokenCookie(result.RefreshToken, persistent: false);
        return Ok(LoginClientResponse.From(result));
    }

    /// <summary>
    /// Logs out the currently authenticated user.
    /// Revokes the current session refresh token and clears the HTTP-only cookie.
    /// Requires a valid Bearer access token.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var rawToken = Request.Cookies[RefreshTokenCookieName];
        await Mediator.Send(new LogoutCommand(rawToken), cancellationToken);

        // Delete the cookie regardless of whether a token was found
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

    /// <summary>
    /// Writes the refresh token into an HTTP-only, Secure, SameSite=Strict cookie.
    /// <paramref name="persistent"/> = true → browser persists the cookie across sessions (RememberMe).
    /// </summary>
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

/// <summary>
/// Client-facing login response. The refresh token is NOT included here —
/// it travels exclusively via the HTTP-only cookie for security.
/// </summary>
public record LoginClientResponse(
    string AccessToken,
    DateTime ExpiresAt,
    int UserId,
    string Username,
    string FullName)
{
    public static LoginClientResponse From(LoginResponse r) =>
        new(r.AccessToken, r.ExpiresAt, r.UserId, r.Username, r.FullName);
}

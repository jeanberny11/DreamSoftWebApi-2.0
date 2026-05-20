using DreamSoft.Application.Features.Apps.ErpApp.Auth;
using DreamSoft.Application.Features.Apps.ErpApp.Auth.LoginBySubdomain;
using DreamSoft.Application.Features.Apps.ErpApp.Auth.LoginByTenantEmail;
using DreamSoft.Application.Features.Apps.ErpApp.Auth.Logout;
using DreamSoft.Application.Features.Apps.ErpApp.Auth.RefreshToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.ErpApp;

public class AuthController : ApiControllerBase
{
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
    /// Reads the refresh token from the HTTP-only cookie.
    /// Returns a new access token in the body; sets a new refresh-token cookie.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginClientResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var rawToken = Request.Cookies[RefreshTokenCookieName];
        if (string.IsNullOrWhiteSpace(rawToken))
            return Unauthorized();

        var result = await Mediator.Send(new RefreshTokenCommand(rawToken), cancellationToken);
        SetRefreshTokenCookie(result.RefreshToken, persistent: false);
        return Ok(LoginClientResponse.From(result));
    }

    /// <summary>
    /// Logs out the currently authenticated solution user.
    /// Revokes the current session refresh token and clears the HTTP-only cookie.
    /// Requires a valid User Bearer token.
    /// </summary>
    [Authorize(Policy = AuthPolicies.UserOnly)]
    [HttpPost("logout")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var rawToken = Request.Cookies[RefreshTokenCookieName];
        await Mediator.Send(new LogoutCommand(rawToken), cancellationToken);

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

public record LoginClientResponse(
    string AccessToken,
    DateTime ExpiresAt,
    int UserId,
    string Username,
    string FullName,
    string TenantSubdomain)
{
    public static LoginClientResponse From(LoginResponse r) =>
        new(r.AccessToken, r.ExpiresAt, r.UserId, r.Username, r.FullName, r.TenantSubdomain);
}

using DreamSoft.Application.Features.Authentication.Login;
using DreamSoft.Application.Features.Authentication.RefreshToken;
using DreamSoft.Application.Features.Authentication.RegisterTenant;
using DreamSoft.Application.Features.Authentication.Requests;
using DreamSoft.Application.Features.Authentication.SendVerificationCode;
using DreamSoft.Application.Features.Authentication.VerifyCode;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers;

/// <summary>
/// Authentication controller - handles registration, login, and token management
/// Uses subdomain-based multi-tenancy (e.g., acme.dreamsoft.com)
/// </summary>
[Route("api/auth")]
public class AuthenticationController : ApiControllerBase
{
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(ILogger<AuthenticationController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Step 1: Send verification code to email
    /// Rate limit: 3 requests per hour per IP
    /// </summary>
    /// <param name="request">Email to send verification code</param>
    /// <returns>Success message with expiration time</returns>
    [HttpPost("send-verification-code")]
    [ProducesResponseType(typeof(SendVerificationCodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SendVerificationCode([FromBody] SendVerificationCodeRequest request)
    {
        var response = await Mediator.Send(request);
        return Ok(response);
    }

    /// <summary>
    /// Step 2: Verify the OTP code and get session token
    /// Max 5 attempts per code
    /// </summary>
    /// <param name="request">Email and verification code</param>
    /// <returns>Session token (valid for 30 minutes)</returns>
    [HttpPost("verify-code")]
    [ProducesResponseType(typeof(VerifyCodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
    {
        var response = await Mediator.Send(request);
        return Ok(response);
    }

    /// <summary>
    /// Check if subdomain is available
    /// </summary>
    /// <param name="request">Subdomain to check</param>
    /// <returns>Availability status</returns>
    [HttpPost("check-subdomain")]
    [ProducesResponseType(typeof(CheckSubdomainResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckSubdomain([FromBody] CheckSubdomainRequest request)
    {
        var response = await Mediator.Send(request);
        return Ok(response);
    }

    /// <summary>
    /// Step 3: Complete registration with session token
    /// Creates tenant and admin user
    /// </summary>
    /// <param name="request">Registration details including session token</param>
    /// <returns>Access token and user/tenant information</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterTenantResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterTenantRequest request)
    {
        var response = await Mediator.Send(request);

        // Set refresh token as HTTP-only cookie
        SetRefreshTokenCookie(response.RefreshToken);

        _logger.LogInformation(
            "New tenant registered: {TenantId}, Subdomain: {Subdomain}",
            response.TenantId, response.Subdomain);

        return CreatedAtAction(nameof(Register), new { id = response.UserId }, response);
    }

    /// <summary>
    /// Login with subdomain-based tenant identification
    /// Subdomain is extracted from request hostname (e.g., acme.dreamsoft.com)
    /// </summary>
    /// <param name="request">Username and password</param>
    /// <returns>Access token and user/tenant information</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await Mediator.Send(request);

        // Set refresh token as HTTP-only cookie
        SetRefreshTokenCookie(response.RefreshToken);

        _logger.LogInformation(
            "User logged in: UserId: {UserId}, TenantId: {TenantId}",
            response.UserId, response.TenantId);

        return Ok(response);
    }

    /// <summary>
    /// Refresh access token using refresh token from HTTP-only cookie
    /// Implements token rotation: old refresh token is revoked, new one is issued
    /// </summary>
    /// <returns>New access token</returns>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken()
    {
        // Extract refresh token from HTTP-only cookie
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogWarning("Refresh token request without cookie");
            return Unauthorized(new { message = "Refresh token is required" });
        }

        var request = new RefreshTokenRequest
        {
            RefreshToken = refreshToken
        };

        var response = await Mediator.Send(request);

        // Set new refresh token as HTTP-only cookie (token rotation)
        SetRefreshTokenCookie(response.RefreshToken);

        _logger.LogInformation("Access token refreshed successfully");

        return Ok(response);
    }

    /// <summary>
    /// Logout - revokes refresh token
    /// Requires authentication
    /// </summary>
    /// <returns>Success message</returns>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Logout()
    {
        // Clear refresh token cookie
        Response.Cookies.Delete("refreshToken");

        _logger.LogInformation("User logged out successfully");

        return Ok(new { success = true, message = "Logged out successfully" });
    }

    /// <summary>
    /// Sets refresh token as HTTP-only cookie with security settings
    /// </summary>
    /// <param name="refreshToken">Refresh token to set</param>
    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,                        // Cannot be accessed by JavaScript (XSS protection)
            Secure = true,                          // Only sent over HTTPS
            SameSite = SameSiteMode.Strict,         // CSRF protection
            Expires = DateTimeOffset.UtcNow.AddDays(7) // 7 days (matches refresh token expiration)
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}

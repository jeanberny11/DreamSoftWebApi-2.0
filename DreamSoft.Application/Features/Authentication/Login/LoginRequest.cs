using MediatR;

namespace DreamSoft.Application.Features.Authentication.Login;

/// <summary>
/// Request to login to the system
/// Subdomain is extracted from HTTP request (e.g., acme.dreamsoft.com → "acme")
/// </summary>
public class LoginRequest : IRequest<LoginResponse>
{
    /// <summary>
    /// User username (unique per tenant)
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// User password
    /// </summary>
    public string Password { get; set; } = null!;

    /// <summary>
    /// Remember me for extended refresh token expiration (30 days vs 7 days)
    /// </summary>
    public bool RememberMe { get; set; } = false;
}

using MediatR;

namespace DreamSoft.Application.Features.Authentication.Login;

/// <summary>
/// Request to login to the system
/// </summary>
public class LoginRequest:IRequest<LoginResponse>
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public bool RememberMe { get; set; } = false; // For extended refresh token expiration
}
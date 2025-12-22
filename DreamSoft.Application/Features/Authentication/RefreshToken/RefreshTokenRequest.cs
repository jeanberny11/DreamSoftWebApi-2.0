using MediatR;

namespace DreamSoft.Application.Features.Authentication.RefreshToken;

/// <summary>
/// Request to refresh access token
/// Refresh token is extracted from HTTP-only cookie by controller and passed here
/// </summary>
public class RefreshTokenRequest : IRequest<RefreshTokenResponse>
{
    /// <summary>
    /// Refresh token (extracted from HTTP-only cookie by controller)
    /// </summary>
    public string RefreshToken { get; set; } = null!;
}

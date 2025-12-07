using MediatR;

namespace DreamSoft.Application.Features.Authentication.RefreshToken;

/// <summary>
/// Request to refresh access token
/// Note: Refresh token comes from httpOnly cookie, not request body
/// </summary>
public class RefreshTokenRequest : IRequest<RefreshTokenResponse>
{
    // Empty - refresh token is read from cookie in controller
    // This is just for consistent command pattern
}
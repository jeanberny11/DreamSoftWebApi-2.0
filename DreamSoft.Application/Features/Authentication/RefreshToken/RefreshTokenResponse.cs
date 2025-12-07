namespace DreamSoft.Application.Features.Authentication.RefreshToken;

/// <summary>
/// Response after successful token refresh
/// </summary>
public class RefreshTokenResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public string AccessToken { get; set; } = null!;
    public int AccessTokenExpiresInSeconds { get; set; }

    // New refresh token will be in httpOnly cookie (not in response body)
}
namespace DreamSoft.Application.Features.Auth;

/// <summary>
/// Shared response returned by both login endpoints and the refresh-token endpoint.
/// Only returned on full successful login — partial/blocked states throw ForbiddenException.
/// </summary>
public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    int UserId,
    string Username,
    string FullName);

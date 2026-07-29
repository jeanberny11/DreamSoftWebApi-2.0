using MediatR;

namespace DreamSoft.Application.Features.Apps.ErpApp.Auth.Logout;

/// <summary>
/// Revokes the authenticated user's current refresh token session.
/// If RefreshToken is provided, only that specific session is revoked.
/// If null, all active sessions for the user are revoked (global logout).
/// Requires a valid access token (Authorize attribute on the controller action).
/// </summary>
public record LogoutCommand(string? RefreshToken = null) : IRequest;

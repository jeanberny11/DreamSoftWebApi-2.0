using MediatR;

namespace DreamSoft.Application.Features.Apps.ErpApp.Auth.RefreshToken;

/// <summary>
/// Rotates a refresh token — invalidates the current one and issues a new access + refresh token pair.
/// </summary>
public record RefreshTokenCommand(string RefreshToken) : IRequest<LoginResponse>;

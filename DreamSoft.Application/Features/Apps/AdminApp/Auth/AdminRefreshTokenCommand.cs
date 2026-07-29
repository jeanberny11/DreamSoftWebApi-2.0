using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Auth;

public record AdminRefreshTokenCommand(string RefreshToken) : IRequest<AdminLoginResponse>;

using MediatR;

namespace DreamSoft.Application.Features.Admin.Auth;

public record AdminRefreshTokenCommand(string RefreshToken) : IRequest<AdminLoginResponse>;

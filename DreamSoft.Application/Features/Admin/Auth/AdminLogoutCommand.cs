using MediatR;

namespace DreamSoft.Application.Features.Admin.Auth;

public record AdminLogoutCommand(string RefreshToken) : IRequest;

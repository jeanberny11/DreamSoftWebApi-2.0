using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Auth;

public record AdminLogoutCommand(string RefreshToken) : IRequest;

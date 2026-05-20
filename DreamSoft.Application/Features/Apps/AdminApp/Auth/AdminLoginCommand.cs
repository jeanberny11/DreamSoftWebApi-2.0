using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Auth;

public record AdminLoginCommand(
    string Email,
    string Password,
    string? DeviceInfo = null) : IRequest<AdminLoginResponse>;

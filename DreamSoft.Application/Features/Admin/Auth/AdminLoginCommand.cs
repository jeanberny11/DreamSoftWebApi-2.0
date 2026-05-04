using MediatR;

namespace DreamSoft.Application.Features.Admin.Auth;

public record AdminLoginCommand(
    string Email,
    string Password,
    string? DeviceInfo = null) : IRequest<AdminLoginResponse>;

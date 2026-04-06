using MediatR;

namespace DreamSoft.Application.Features.Auth.LogoutTenant;

public record LogoutTenantCommand(string? RefreshToken) : IRequest;

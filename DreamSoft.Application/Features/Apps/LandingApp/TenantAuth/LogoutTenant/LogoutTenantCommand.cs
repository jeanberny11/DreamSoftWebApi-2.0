using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.TenantAuth.LogoutTenant;

public record LogoutTenantCommand(string? RefreshToken) : IRequest;

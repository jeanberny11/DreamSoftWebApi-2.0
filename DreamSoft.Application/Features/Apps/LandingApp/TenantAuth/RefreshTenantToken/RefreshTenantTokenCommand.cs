using DreamSoft.Application.Features.Apps.LandingApp.TenantAuth.Dtos;
using DreamSoft.Application.Features.Apps.LandingApp.TenantAuth.LoginTenant;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.TenantAuth.RefreshTenantToken;

public record RefreshTenantTokenCommand(string? RefreshToken) : IRequest<TenantAuthResponse>;
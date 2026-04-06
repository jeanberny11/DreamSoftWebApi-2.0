using DreamSoft.Application.Features.Auth.LoginTenant;
using MediatR;

namespace DreamSoft.Application.Features.Auth.RefreshTenantToken;

public record RefreshTenantTokenCommand(string? RefreshToken) : IRequest<LoginTenantResponse>;

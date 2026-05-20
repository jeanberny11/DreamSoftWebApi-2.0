using MediatR;

namespace DreamSoft.Application.Features.Apps.ErpApp.Auth.LoginByTenantEmail;

/// <summary>
/// Authenticates a user by resolving the Tenant from the company email (Tenant.Email),
/// then validating the username and password within that tenant.
/// Intended for clients that don't use subdomain-based routing (e.g. mobile apps).
/// </summary>
public record LoginByTenantEmailCommand(
    string TenantEmail,
    string Username,
    string Password,
    bool RememberMe = false,
    string? DeviceInfo = null) : IRequest<LoginResponse>;

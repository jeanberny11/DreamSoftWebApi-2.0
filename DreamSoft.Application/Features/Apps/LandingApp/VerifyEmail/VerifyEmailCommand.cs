using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.VerifyEmail;

/// <summary>
/// Tenant is identified from the Bearer token — no body fields required.
/// Code is the 6-digit OTP sent to that address.
/// No Authorization header or registration JWT is required.
/// </summary>
public record VerifyEmailCommand(
    string Code
) : IRequest<VerifyEmailResponse>;

public record VerifyEmailResponse(
    int TenantId,
    string Email
);

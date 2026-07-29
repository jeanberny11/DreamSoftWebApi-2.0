using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.VerifyEmail;

/// <summary>
/// Tenant is identified from the Bearer token — no body fields required.
/// Code is the 6-digit OTP sent to that address.
/// Requires a valid tenant Bearer token.
/// </summary>
public record VerifyEmailCommand(
    string Code
) : IRequest<VerifyEmailResponse>;

public record VerifyEmailResponse(
    string Email,
    bool EmailVerified,
    string StatusCode
);

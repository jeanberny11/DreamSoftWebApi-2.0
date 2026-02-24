using MediatR;

namespace DreamSoft.Application.Features.Registration.ResendVerification;

/// <summary>
/// Email identifies the tenant whose OTP should be resent.
/// No Authorization header or registration JWT is required.
/// </summary>
public record ResendVerificationCommand(
    string Email
) : IRequest<Unit>;

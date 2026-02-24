using MediatR;

namespace DreamSoft.Application.Features.Registration.VerifyEmail;

/// <summary>
/// Email identifies the tenant (normalized to lowercase).
/// Code is the 6-digit OTP sent to that address.
/// No Authorization header or registration JWT is required.
/// </summary>
public record VerifyEmailCommand(
    string Email,
    string Code
) : IRequest<VerifyEmailResponse>;

public record VerifyEmailResponse(string AccessToken, string RefreshToken);

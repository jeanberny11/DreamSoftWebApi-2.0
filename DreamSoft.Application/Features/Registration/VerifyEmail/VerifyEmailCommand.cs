using MediatR;

namespace DreamSoft.Application.Features.Registration.VerifyEmail;

/// <summary>
/// RegistrationToken is extracted from the Authorization: Bearer header
/// by the controller and passed in here. It is NOT part of the request body.
/// </summary>
public record VerifyEmailCommand(
    string RegistrationToken,
    string Code
) : IRequest<VerifyEmailResponse>;

public record VerifyEmailResponse(string AccessToken, string RefreshToken);

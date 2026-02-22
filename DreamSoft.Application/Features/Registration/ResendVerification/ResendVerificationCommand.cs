using MediatR;

namespace DreamSoft.Application.Features.Registration.ResendVerification;

/// <summary>
/// RegistrationToken is extracted from the Authorization header by the controller.
/// </summary>
public record ResendVerificationCommand(
    string RegistrationToken
) : IRequest<Unit>;

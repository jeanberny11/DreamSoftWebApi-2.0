using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.SendEmailVerificationCode;

public record SendEmailVerificationCodeResponse(
    string Email,
    bool Success,
    string Message,
    DateTime ExpiresAt
);

/// <summary>
/// Tenant is identified from the Bearer token — no body fields required.
/// </summary>
public record SendEmailVerificationCodeCommand : IRequest<SendEmailVerificationCodeResponse>;

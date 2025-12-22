using FluentValidation;

namespace DreamSoft.Application.Features.Authentication.RefreshToken;

/// <summary>
/// Validator for RefreshTokenRequest
/// </summary>
public class RefreshTokenValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required")
            .MinimumLength(32)
            .WithMessage("Invalid refresh token format")
            .MaximumLength(512)
            .WithMessage("Invalid refresh token format");
    }
}

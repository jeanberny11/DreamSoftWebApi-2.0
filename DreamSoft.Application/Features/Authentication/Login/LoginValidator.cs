using DreamSoft.Application.Common.Validators;
using FluentValidation;

namespace DreamSoft.Application.Features.Authentication.Login;

/// <summary>
/// Validator for LoginRequest
/// </summary>
public class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required.");

        RuleFor(x => x.Password)
            .BasicPassword(); // ✅ Using custom validator

        // RememberMe is optional, no validation needed
    }
}
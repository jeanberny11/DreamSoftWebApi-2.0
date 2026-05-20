using FluentValidation;

namespace DreamSoft.Application.Features.Apps.ErpApp.Auth.LoginBySubdomain;

public class LoginBySubdomainCommandValidator : AbstractValidator<LoginBySubdomainCommand>
{
    public LoginBySubdomainCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(100).WithMessage("Username must not exceed 100 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MaximumLength(255).WithMessage("Password must not exceed 255 characters.");
    }
}

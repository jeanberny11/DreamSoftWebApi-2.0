using FluentValidation;

namespace DreamSoft.Application.Features.Auth.LoginByTenantEmail;

public class LoginByTenantEmailCommandValidator : AbstractValidator<LoginByTenantEmailCommand>
{
    public LoginByTenantEmailCommandValidator()
    {
        RuleFor(x => x.TenantEmail)
            .NotEmpty().WithMessage("Tenant email is required.")
            .EmailAddress().WithMessage("Tenant email must be a valid email address.")
            .MaximumLength(255).WithMessage("Tenant email must not exceed 255 characters.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(100).WithMessage("Username must not exceed 100 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MaximumLength(255).WithMessage("Password must not exceed 255 characters.");
    }
}

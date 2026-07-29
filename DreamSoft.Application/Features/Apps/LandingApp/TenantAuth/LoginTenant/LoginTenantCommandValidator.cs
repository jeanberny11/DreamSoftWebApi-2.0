using FluentValidation;

namespace DreamSoft.Application.Features.Apps.LandingApp.TenantAuth.LoginTenant;

public class LoginTenantCommandValidator : AbstractValidator<LoginTenantCommand>
{
    public LoginTenantCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MaximumLength(255).WithMessage("Password must not exceed 255 characters.");
    }
}

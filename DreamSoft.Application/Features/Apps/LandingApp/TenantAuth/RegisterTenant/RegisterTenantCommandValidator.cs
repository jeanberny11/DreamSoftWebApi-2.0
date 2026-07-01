using DreamSoft.Application.Common.Validators;
using FluentValidation;

namespace DreamSoft.Application.Features.Apps.LandingApp.Registration.RegisterTenant;

public class RegisterTenantCommandValidator
    : AbstractValidator<RegisterTenantCommand>
{
    public RegisterTenantCommandValidator()
    {
        RuleFor(x => x.FirstName).PersonName();
        RuleFor(x => x.LastName).PersonName();
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required")
            .MaximumLength(255).WithMessage("Company name must not exceed 255 characters");
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.Password).StrongPassword();

        RuleFor(x => x.AcceptTerms)
            .MustAcceptTerms()
            .When(x => !string.IsNullOrWhiteSpace(x.TermsVersion));
    }
}

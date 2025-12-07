using DreamSoft.Application.Common.Validators;
using DreamSoft.Application.Features.Authentication.Requests;
using FluentValidation;

namespace DreamSoft.Application.Features.Authentication.Validators;

/// <summary>
/// Validator for RegisterTenantRequest
/// </summary>
public class RegisterTenantValidator : AbstractValidator<RegisterTenantRequest>
{
    public RegisterTenantValidator()
    {
        // Session token validation
        RuleFor(x => x.SessionToken)
            .NotEmpty()
            .WithMessage("Session token is required");

        // Company information validation
        RuleFor(x => x.CompanyName)
            .CompanyName(); // ✅ Using custom validator

        RuleFor(x => x.Subdomain)
            .SubdomainFormat(); // ✅ Using custom validator

        RuleFor(x => x.Phone!)
            .InternationalPhone() // ✅ Using custom validator
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.TaxId!)
            .TaxId() // ✅ Using custom validator
            .When(x => !string.IsNullOrWhiteSpace(x.TaxId));

        // User information validation
        RuleFor(x => x.FirstName)
            .PersonName(); // ✅ Using custom validator

        RuleFor(x => x.LastName)
            .PersonName(); // ✅ Using custom validator

        // Password validation
        RuleFor(x => x.Password)
            .StrongPassword(); // ✅ Using custom validator

        RuleFor(x => x.ConfirmPassword)
            .PasswordConfirmation(x => x.Password); // ✅ Using custom validator

        // Language validation
        RuleFor(x => x.LanguageId)
            .PositiveId(); // ✅ Using custom validator

        // Terms acceptance validation
        RuleFor(x => x.AcceptedTerms)
            .MustAcceptTerms(); // ✅ Using custom validator
    }
}
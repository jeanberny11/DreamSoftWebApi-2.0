using DreamSoft.Application.Common.Validators;
using FluentValidation;

namespace DreamSoft.Application.Features.Onboarding.CompleteOnboarding;

public class CompleteOnboardingCommandValidator
    : AbstractValidator<CompleteOnboardingCommand>
{
    public CompleteOnboardingCommandValidator()
    {
        // Website — optional, max length if provided
        RuleFor(x => x.Website)
            .MaximumLength(255)
            .WithMessage("Website must not exceed 255 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Website));

        // Address fields — optional, max lengths if provided
        RuleFor(x => x.AddressLine1)
            .MaximumLength(255)
            .WithMessage("Address line 1 must not exceed 255 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.AddressLine1));

        RuleFor(x => x.AddressLine2)
            .MaximumLength(255)
            .WithMessage("Address line 2 must not exceed 255 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.AddressLine2));

        RuleFor(x => x.PostalCode)
            .MaximumLength(20)
            .WithMessage("Postal code must not exceed 20 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.PostalCode));

        // FK IDs — must be positive if provided
        RuleFor(x => x.CountryId)
            .GreaterThan(0)
            .WithMessage("Country ID must be greater than 0")
            .When(x => x.CountryId.HasValue);

        RuleFor(x => x.ProvinceId)
            .GreaterThan(0)
            .WithMessage("Province ID must be greater than 0")
            .When(x => x.ProvinceId.HasValue);

        RuleFor(x => x.MunicipalityId)
            .GreaterThan(0)
            .WithMessage("Municipality ID must be greater than 0")
            .When(x => x.MunicipalityId.HasValue);

        // Language — required, must be a positive ID
        RuleFor(x => x.LanguageId)
            .PositiveId();
    }
}

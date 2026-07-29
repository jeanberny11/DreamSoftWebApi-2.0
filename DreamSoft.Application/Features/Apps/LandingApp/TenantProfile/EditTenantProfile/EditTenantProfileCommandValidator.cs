using DreamSoft.Application.Common.Validators;
using FluentValidation;

namespace DreamSoft.Application.Features.Apps.LandingApp.TenantProfile.EditTenantProfile;

public class EditTenantProfileCommandValidator : AbstractValidator<EditTenantProfileCommand>
{
    public EditTenantProfileCommandValidator()
    {
        RuleFor(x => x.FirstName).PersonName();
        RuleFor(x => x.LastName).PersonName();
        RuleFor(x => x.CompanyName).CompanyName();

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .InternationalPhone();

        RuleFor(x => x.Website)
            .MaximumLength(255).WithMessage("Website must not exceed 255 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Website));

        RuleFor(x => x.AddressLine1)
            .NotEmpty().WithMessage("Address line 1 is required")
            .MaximumLength(255).WithMessage("Address line 1 must not exceed 255 characters");

        RuleFor(x => x.AddressLine2)
            .MaximumLength(255).WithMessage("Address line 2 must not exceed 255 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.AddressLine2));

        RuleFor(x => x.PostalCode)
            .MaximumLength(20).WithMessage("Postal code must not exceed 20 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.PostalCode));

        RuleFor(x => x.CountryId).PositiveId();
        RuleFor(x => x.ProvinceId).PositiveId();
        RuleFor(x => x.MunicipalityId).PositiveId();
        RuleFor(x => x.LanguageId).PositiveId();
    }
}

using DreamSoft.Application.Common.Validators;
using FluentValidation;

namespace DreamSoft.Application.Features.Registration.RegisterTenant;

public class RegisterTenantCommandValidator
    : AbstractValidator<RegisterTenantCommand>
{
    public RegisterTenantCommandValidator()
    {
        RuleFor(x => x.CompanyName).CompanyName();
        RuleFor(x => x.Subdomain).SubdomainFormat();

        RuleFor(x => x.TaxId!).TaxId().When(x => x.TaxId != null);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .InternationalPhone();

        RuleFor(x => x.AddressLine1)
            .NotEmpty().WithMessage("Address line 1 is required")
            .MaximumLength(255).WithMessage("Address line 1 must not exceed 255 characters");

        RuleFor(x => x.CountryId).PositiveId();
        RuleFor(x => x.ProvinceId).PositiveId();
        RuleFor(x => x.MunicipalityId).PositiveId();

        RuleFor(x => x.AdminFirstName).PersonName();
        RuleFor(x => x.AdminLastName).PersonName();
        RuleFor(x => x.AdminEmail).ValidEmail();
        RuleFor(x => x.AdminPassword).StrongPassword();

        RuleFor(x => x.LanguageId).PositiveId();
        RuleFor(x => x.CurrencyId).PositiveId();
    }
}

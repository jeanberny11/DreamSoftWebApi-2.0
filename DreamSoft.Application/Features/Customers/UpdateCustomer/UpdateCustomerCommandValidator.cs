using DreamSoft.Application.Common.Validators;
using FluentValidation;

namespace DreamSoft.Application.Features.Customers.UpdateCustomer;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId).PositiveId();
        RuleFor(x => x.CustomerStatusId).PositiveId();
        RuleFor(x => x.CountryId).PositiveId();
        RuleFor(x => x.ProvinceId).PositiveId();
        RuleFor(x => x.MunicipalityId).PositiveId();
        RuleFor(x => x.CurrencyId).PositiveId();

        // At least one contact method required
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Email)
                    || !string.IsNullOrWhiteSpace(x.Phone)
                    || !string.IsNullOrWhiteSpace(x.Mobile))
            .WithName("ContactMethod")
            .WithMessage("At least one contact method is required: Email, Phone, or Mobile.");

        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
            RuleFor(x => x.Email!).BasicEmail());

        When(x => !string.IsNullOrWhiteSpace(x.Phone), () =>
            RuleFor(x => x.Phone!).InternationalPhone());

        When(x => !string.IsNullOrWhiteSpace(x.Mobile), () =>
            RuleFor(x => x.Mobile!).InternationalPhone());

        When(x => !string.IsNullOrWhiteSpace(x.FirstName), () =>
            RuleFor(x => x.FirstName!).PersonName());

        When(x => !string.IsNullOrWhiteSpace(x.LastName), () =>
            RuleFor(x => x.LastName!).PersonName());

        When(x => !string.IsNullOrWhiteSpace(x.CompanyName), () =>
            RuleFor(x => x.CompanyName!).CompanyName());

        When(x => !string.IsNullOrWhiteSpace(x.TaxId), () =>
            RuleFor(x => x.TaxId!).TaxId());

        When(x => !string.IsNullOrWhiteSpace(x.Website), () =>
            RuleFor(x => x.Website!)
                .MaximumLength(255).WithMessage("Website must not exceed 255 characters"));

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Credit limit cannot be negative");

        RuleFor(x => x.DiscountPercentage)
            .InclusiveBetween(0, 100).WithMessage("Discount percentage must be between 0 and 100");
    }
}

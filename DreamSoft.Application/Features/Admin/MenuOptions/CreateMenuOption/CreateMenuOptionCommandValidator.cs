using FluentValidation;

namespace DreamSoft.Application.Features.Admin.MenuOptions.CreateMenuOption;

public class CreateMenuOptionCommandValidator : AbstractValidator<CreateMenuOptionCommand>
{
    public CreateMenuOptionCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description must not exceed 200 characters.");

        RuleFor(x => x.ModuleId)
            .GreaterThan(0).WithMessage("Module ID must be greater than zero.");

        RuleFor(x => x.MenuGroupId)
            .GreaterThan(0).WithMessage("Menu group ID must be greater than zero.");

        RuleFor(x => x.Route)
            .MaximumLength(255).WithMessage("Route must not exceed 255 characters.");

        RuleFor(x => x.Icon)
            .MaximumLength(50).WithMessage("Icon must not exceed 50 characters.");

        RuleFor(x => x.Translations)
            .NotNull().WithMessage("Translations are required.");

        RuleFor(x => x.Translations.Spanish)
            .NotNull().WithMessage("Spanish translation is required.");

        RuleFor(x => x.Translations.Spanish.Name)
            .NotEmpty().WithMessage("Spanish name is required.")
            .MaximumLength(50).WithMessage("Spanish name must not exceed 50 characters.");

        RuleFor(x => x.Translations.Spanish.Description)
            .MaximumLength(200).WithMessage("Spanish description must not exceed 200 characters.")
            .When(x => x.Translations?.Spanish?.Description is not null);

        RuleFor(x => x.Translations.English!.Name)
            .MaximumLength(50).WithMessage("English name must not exceed 50 characters.")
            .When(x => x.Translations?.English is not null);

        RuleFor(x => x.Translations.English!.Description)
            .MaximumLength(200).WithMessage("English description must not exceed 200 characters.")
            .When(x => x.Translations?.English?.Description is not null);
    }
}

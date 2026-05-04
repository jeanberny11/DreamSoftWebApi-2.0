using FluentValidation;

namespace DreamSoft.Application.Features.Admin.Solutions.CreateSolution;

public class CreateSolutionCommandValidator : AbstractValidator<CreateSolutionCommand>
{
    public CreateSolutionCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters.");

        RuleFor(x => x.Translations)
            .NotNull().WithMessage("Translations are required.");

        RuleFor(x => x.Translations.Spanish)
            .NotNull().WithMessage("Spanish translation is required.");

        RuleFor(x => x.Translations.Spanish.Name)
            .NotEmpty().WithMessage("Spanish name is required.")
            .MaximumLength(50).WithMessage("Spanish name must not exceed 50 characters.");

        RuleFor(x => x.Translations.Spanish.Description)
            .MaximumLength(500).WithMessage("Spanish description must not exceed 500 characters.")
            .When(x => x.Translations?.Spanish?.Description is not null);

        RuleFor(x => x.Translations.English!.Name)
            .MaximumLength(50).WithMessage("English name must not exceed 50 characters.")
            .When(x => x.Translations?.English is not null);

        RuleFor(x => x.Translations.English!.Description)
            .MaximumLength(500).WithMessage("English description must not exceed 500 characters.")
            .When(x => x.Translations?.English?.Description is not null);

        RuleFor(x => x.Icon)
            .MaximumLength(50).WithMessage("Icon must not exceed 50 characters.")
            .When(x => x.Icon is not null);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("SortOrder must be a non-negative integer.");
    }
}

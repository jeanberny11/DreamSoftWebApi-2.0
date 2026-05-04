using FluentValidation;

namespace DreamSoft.Application.Features.Admin.Genders.UpdateGender;

public class UpdateGenderCommandValidator : AbstractValidator<UpdateGenderCommand>
{
    public UpdateGenderCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Translations)
            .NotNull().WithMessage("Translations are required.");

        RuleFor(x => x.Translations.Spanish)
            .NotNull().WithMessage("Spanish translation is required.");

        RuleFor(x => x.Translations.Spanish.Name)
            .NotEmpty().WithMessage("Spanish name is required.")
            .MaximumLength(100).WithMessage("Spanish name must not exceed 100 characters.");

        RuleFor(x => x.Translations.English!.Name)
            .MaximumLength(100).WithMessage("English name must not exceed 100 characters.")
            .When(x => x.Translations?.English is not null);
    }
}

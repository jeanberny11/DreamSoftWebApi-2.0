using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Admin.BillingCycles.GetBillingCycles;
using DreamSoft.Domain.Repositories;
using DreamSoft.Domain.ValueObjects;
using MediatR;

namespace DreamSoft.Application.Features.Admin.BillingCycles.UpdateBillingCycle;

public class UpdateBillingCycleCommandHandler(
    IBillingCycleRepository billingCycleRepository,
    IRequestLanguageService languageService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateBillingCycleCommand, BillingCycleDto>
{
    public async Task<BillingCycleDto> Handle(
        UpdateBillingCycleCommand request, CancellationToken cancellationToken)
    {
        var cycle = await billingCycleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "BillingCycle", request.Id);

        var translations = TranslatedString.Create(
            BaseTranslatedProperties.Create(
                request.Translations.Spanish.Name,
                request.Translations.Spanish.Description),
            request.Translations.English is not null
                ? BaseTranslatedProperties.Create(
                    request.Translations.English.Name,
                    request.Translations.English.Description)
                : null);

        cycle.UpdateDetails(request.Name, request.Description ?? string.Empty, translations);
        cycle.SetActive(request.IsActive);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var language = languageService.Resolve();

        return new BillingCycleDto(
            cycle.Id, cycle.Code,
            cycle.Translations.GetNameOrFallback(language, cycle.Name),
            cycle.Translations.GetDescriptionOrFallback(language, cycle.Description),
            cycle.Months, cycle.IsActive);
    }
}

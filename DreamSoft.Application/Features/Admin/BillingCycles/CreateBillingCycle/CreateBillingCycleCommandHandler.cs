using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Admin.BillingCycles.GetBillingCycles;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using DreamSoft.Domain.ValueObjects;
using MediatR;

namespace DreamSoft.Application.Features.Admin.BillingCycles.CreateBillingCycle;

public class CreateBillingCycleCommandHandler(
    IBillingCycleRepository billingCycleRepository,
    IRequestLanguageService languageService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateBillingCycleCommand, BillingCycleDto>
{
    public async Task<BillingCycleDto> Handle(
        CreateBillingCycleCommand request, CancellationToken cancellationToken)
    {
        var exists = await billingCycleRepository.AnyAsync(
            c => c.Code == request.Code.ToUpper().Trim(), cancellationToken);

        if (exists)
            throw new ConflictException("BillingCycleCodeAlreadyExists", request.Code);

        var translations = TranslatedString.Create(
            BaseTranslatedProperties.Create(
                request.Translations.Spanish.Name,
                request.Translations.Spanish.Description),
            request.Translations.English is not null
                ? BaseTranslatedProperties.Create(
                    request.Translations.English.Name,
                    request.Translations.English.Description)
                : null);

        var cycle = BillingCycle.Create(
            request.Code,
            request.Name,
            request.Description ?? string.Empty,
            translations,
            request.Months);

        await billingCycleRepository.AddAsync(cycle, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var language = languageService.Resolve();

        return new BillingCycleDto(
            cycle.Id, cycle.Code,
            cycle.Translations.GetNameOrFallback(language, cycle.Name),
            cycle.Translations.GetDescriptionOrFallback(language, cycle.Description),
            cycle.Months, cycle.IsActive);
    }
}

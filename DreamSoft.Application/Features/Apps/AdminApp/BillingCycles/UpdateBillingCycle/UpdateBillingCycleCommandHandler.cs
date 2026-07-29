using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Domain.Repositories;
using DreamSoft.Domain.ValueObjects;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.BillingCycles.UpdateBillingCycle;

public class UpdateBillingCycleCommandHandler(
    IBillingCycleRepository billingCycleRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateBillingCycleCommand, Unit>
{
    public async Task<Unit> Handle(
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

        return Unit.Value;
    }
}

using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Domain.Repositories;
using DreamSoft.Domain.ValueObjects;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.SubscriptionPlans.UpdateSubscriptionPlan;

public class UpdateSubscriptionPlanCommandHandler(
    ISubscriptionPlanRepository planRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSubscriptionPlanCommand, Unit>
{
    public async Task<Unit> Handle(
        UpdateSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await planRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "SubscriptionPlan", request.Id);

        var translations = TranslatedString.Create(
            BaseTranslatedProperties.Create(
                request.Translations.Spanish.Name,
                request.Translations.Spanish.Description),
            request.Translations.English is not null
                ? BaseTranslatedProperties.Create(
                    request.Translations.English.Name,
                    request.Translations.English.Description)
                : null);

        plan.UpdateDetails(request.Name, request.Description ?? string.Empty, translations);
        plan.UpdateTrialDays(request.TrialDays);
        plan.SetActive(request.IsActive);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

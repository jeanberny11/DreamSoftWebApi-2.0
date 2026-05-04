using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Admin.SubscriptionPlans.GetSubscriptionPlans;
using DreamSoft.Domain.Repositories;
using DreamSoft.Domain.ValueObjects;
using MediatR;

namespace DreamSoft.Application.Features.Admin.SubscriptionPlans.UpdateSubscriptionPlan;

public class UpdateSubscriptionPlanCommandHandler(
    ISubscriptionPlanRepository planRepository,
    ISolutionRepository solutionRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSubscriptionPlanCommand, SubscriptionPlanDto>
{
    public async Task<SubscriptionPlanDto> Handle(
        UpdateSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await planRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "SubscriptionPlan", request.Id);

        var solution = await solutionRepository.GetByIdAsync(plan.SolutionId, cancellationToken);

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

        return new SubscriptionPlanDto(
            plan.Id, plan.Code, plan.Name, plan.Description,
            plan.SolutionId, solution?.Code ?? string.Empty,
            plan.TierLevel, plan.TrialDays, plan.IsActive);
    }
}

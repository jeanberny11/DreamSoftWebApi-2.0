using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.AdminApp.SubscriptionPlans.GetSubscriptionPlans;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using DreamSoft.Domain.ValueObjects;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.SubscriptionPlans.CreateSubscriptionPlan;

public class CreateSubscriptionPlanCommandHandler(
    ISubscriptionPlanRepository planRepository,
    ISolutionRepository solutionRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateSubscriptionPlanCommand, SubscriptionPlanDto>
{
    public async Task<SubscriptionPlanDto> Handle(
        CreateSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        var solution = await solutionRepository.GetByIdAsync(request.SolutionId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "Solution", request.SolutionId);

        var exists = await planRepository.AnyAsync(
            p => p.Code == request.Code.ToUpper().Trim(), cancellationToken);

        if (exists)
            throw new ConflictException("SubscriptionPlanCodeAlreadyExists", request.Code);

        var translations = TranslatedString.Create(
            BaseTranslatedProperties.Create(
                request.Translations.Spanish.Name,
                request.Translations.Spanish.Description),
            request.Translations.English is not null
                ? BaseTranslatedProperties.Create(
                    request.Translations.English.Name,
                    request.Translations.English.Description)
                : null);

        var plan = SubscriptionPlan.Create(
            request.Code,
            request.Name,
            translations,
            request.SolutionId,
            request.TierLevel,
            request.Description ?? string.Empty,
            request.TrialDays);

        await planRepository.AddAsync(plan, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SubscriptionPlanDto(
            plan.Id, plan.Code, plan.Name, plan.Description,
            plan.SolutionId, solution.Code,
            plan.TierLevel, plan.TrialDays, plan.IsActive);
    }
}

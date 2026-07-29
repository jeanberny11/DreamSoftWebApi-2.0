using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Features.Apps.AdminApp.PlanMenuOptions.DTOs;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.PlanMenuOptions.SetPlanMenuOptions;

public record SetPlanMenuOptionsCommand(int PlanId, List<int> MenuOptionIds)
    : IRequest<IReadOnlyList<PlanMenuOptionDto>>;

public class SetPlanMenuOptionsCommandHandler(
    IPlanMenuOptionRepository planMenuOptionRepository,
    ISubscriptionPlanRepository planRepository,
    IMenuOptionRepository menuOptionRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SetPlanMenuOptionsCommand, IReadOnlyList<PlanMenuOptionDto>>
{
    public async Task<IReadOnlyList<PlanMenuOptionDto>> Handle(
        SetPlanMenuOptionsCommand request, CancellationToken cancellationToken)
    {
        var plan = await planRepository.GetByIdAsync(request.PlanId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "SubscriptionPlan", request.PlanId);

        var menuOptions = await menuOptionRepository.FindAsync(
            mo => request.MenuOptionIds.Contains(mo.Id), cancellationToken);

        var missingIds = request.MenuOptionIds.Except(menuOptions.Select(m => m.Id)).ToList();
        if (missingIds.Count > 0)
            throw new NotFoundException(ErrorMessageKeys.NotFound,
                $"MenuOptions with IDs [{string.Join(", ", missingIds)}]", request.PlanId);

        var existing = await planMenuOptionRepository.GetByPlanIdAsync(request.PlanId, cancellationToken);
        foreach (var entry in existing)
            await planMenuOptionRepository.DeleteAsync(entry, cancellationToken);

        var newEntries = request.MenuOptionIds
            .Distinct()
            .Select(id => PlanMenuOption.Create(request.PlanId, id))
            .ToList();

        await planMenuOptionRepository.AddRangeAsync(newEntries, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return newEntries
            .Select(e =>
            {
                var mo = menuOptions.First(m => m.Id == e.MenuOptionId);
                return new PlanMenuOptionDto(e.PlanId, e.MenuOptionId, mo.Code, mo.Name);
            })
            .ToList();
    }
}

public class SetPlanMenuOptionsCommandValidator : AbstractValidator<SetPlanMenuOptionsCommand>
{
    public SetPlanMenuOptionsCommandValidator()
    {
        RuleFor(x => x.PlanId).GreaterThan(0).WithMessage("PlanId must be greater than zero.");
        RuleFor(x => x.MenuOptionIds).NotNull().WithMessage("MenuOptionIds is required.");
        RuleForEach(x => x.MenuOptionIds)
            .GreaterThan(0).WithMessage("Each MenuOptionId must be greater than zero.");
    }
}

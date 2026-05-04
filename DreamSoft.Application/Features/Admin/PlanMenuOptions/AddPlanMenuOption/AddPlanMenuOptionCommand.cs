using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Admin.PlanMenuOptions.GetPlanMenuOptions;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace DreamSoft.Application.Features.Admin.PlanMenuOptions.AddPlanMenuOption;

public record AddPlanMenuOptionCommand(int PlanId, int MenuOptionId) : IRequest<PlanMenuOptionDto>;

public class AddPlanMenuOptionCommandHandler(
    IPlanMenuOptionRepository planMenuOptionRepository,
    ISubscriptionPlanRepository planRepository,
    IMenuOptionRepository menuOptionRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddPlanMenuOptionCommand, PlanMenuOptionDto>
{
    public async Task<PlanMenuOptionDto> Handle(
        AddPlanMenuOptionCommand request, CancellationToken cancellationToken)
    {
        var plan = await planRepository.GetByIdAsync(request.PlanId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "SubscriptionPlan", request.PlanId);

        var menuOption = await menuOptionRepository.GetByIdAsync(request.MenuOptionId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "MenuOption", request.MenuOptionId);

        var existingIds = await planMenuOptionRepository.GetMenuOptionIdsByPlanIdAsync(
            request.PlanId, cancellationToken);

        if (existingIds.Contains(request.MenuOptionId))
            throw new ConflictException("PlanMenuOptionAlreadyExists",
                $"Plan {request.PlanId} already includes menu option {request.MenuOptionId}");

        var entry = PlanMenuOption.Create(request.PlanId, request.MenuOptionId);

        await planMenuOptionRepository.AddAsync(entry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new PlanMenuOptionDto(entry.PlanId, entry.MenuOptionId, menuOption.Code, menuOption.Name);
    }
}

public class AddPlanMenuOptionCommandValidator : AbstractValidator<AddPlanMenuOptionCommand>
{
    public AddPlanMenuOptionCommandValidator()
    {
        RuleFor(x => x.PlanId).GreaterThan(0).WithMessage("PlanId must be greater than zero.");
        RuleFor(x => x.MenuOptionId).GreaterThan(0).WithMessage("MenuOptionId must be greater than zero.");
    }
}

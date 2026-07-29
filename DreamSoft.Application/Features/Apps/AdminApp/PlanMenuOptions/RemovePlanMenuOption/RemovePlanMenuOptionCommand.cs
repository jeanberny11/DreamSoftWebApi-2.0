using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.PlanMenuOptions.RemovePlanMenuOption;

public record RemovePlanMenuOptionCommand(int PlanId, int MenuOptionId) : IRequest;

public class RemovePlanMenuOptionCommandHandler(
    IPlanMenuOptionRepository planMenuOptionRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RemovePlanMenuOptionCommand>
{
    public async Task Handle(RemovePlanMenuOptionCommand request, CancellationToken cancellationToken)
    {
        var entries = await planMenuOptionRepository.GetByPlanIdAsync(request.PlanId, cancellationToken);
        var entry = entries.FirstOrDefault(e => e.MenuOptionId == request.MenuOptionId)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound,
                $"MenuOption {request.MenuOptionId} on Plan", request.PlanId);

        await planMenuOptionRepository.DeleteAsync(entry, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

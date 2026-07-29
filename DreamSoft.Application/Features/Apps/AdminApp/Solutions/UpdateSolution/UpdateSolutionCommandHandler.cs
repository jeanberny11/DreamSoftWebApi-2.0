using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Domain.Repositories;
using DreamSoft.Domain.ValueObjects;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Solutions.UpdateSolution;

public class UpdateSolutionCommandHandler(
    ISolutionRepository solutionRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSolutionCommand, Unit>
{
    public async Task<Unit> Handle(
        UpdateSolutionCommand request,
        CancellationToken cancellationToken)
    {
        var solution = await solutionRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "Solution", request.Id);

        var translations = TranslatedString.Create(
            BaseTranslatedProperties.Create(
                request.Translations.Spanish.Name,
                request.Translations.Spanish.Description),
            request.Translations.English is not null
                ? BaseTranslatedProperties.Create(
                    request.Translations.English.Name,
                    request.Translations.English.Description)
                : null);

        solution.UpdateDetails(request.Name, request.Description ?? string.Empty, translations);
        solution.UpdateIcon(request.Icon ?? string.Empty);
        solution.UpdateSortOrder(request.SortOrder);

        if (request.IsActive && !solution.IsActive)
            solution.Activate();
        else if (!request.IsActive && solution.IsActive)
            solution.Deactivate();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

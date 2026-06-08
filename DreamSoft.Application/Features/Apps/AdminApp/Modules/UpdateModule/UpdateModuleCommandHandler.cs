using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Domain.Repositories;
using DreamSoft.Domain.ValueObjects;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Modules.UpdateModule;

public class UpdateModuleCommandHandler(
    IModuleRepository moduleRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateModuleCommand, Unit>
{
    public async Task<Unit> Handle(
        UpdateModuleCommand request,
        CancellationToken cancellationToken)
    {
        var module = await moduleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "Module", request.Id);

        var es = request.Translations.Spanish;
        var en = request.Translations.English;

        var translations = TranslatedString.Create(
            BaseTranslatedProperties.Create(es.Name, es.Description),
            en is not null ? BaseTranslatedProperties.Create(en.Name, en.Description) : null);

        module.UpdateDetails(request.Name, request.Description, translations);
        module.UpdateIcon(request.Icon);
        module.UpdateSortOrder(request.SortOrder);

        if (request.IsActive)
            module.Activate();
        else
            module.Deactivate();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

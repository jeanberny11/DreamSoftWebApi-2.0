using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Domain.Repositories;
using DreamSoft.Domain.ValueObjects;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.MenuGroups.UpdateMenuGroup;

public class UpdateMenuGroupCommandHandler(
    IMenuGroupRepository menuGroupRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateMenuGroupCommand, Unit>
{
    public async Task<Unit> Handle(
        UpdateMenuGroupCommand request,
        CancellationToken cancellationToken)
    {
        var menuGroup = await menuGroupRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "MenuGroup", request.Id);

        var es = request.Translations.Spanish;
        var en = request.Translations.English;

        var translations = TranslatedString.Create(
            BaseTranslatedProperties.Create(es.Name, es.Description),
            en is not null ? BaseTranslatedProperties.Create(en.Name, en.Description) : null);

        menuGroup.UpdateDetails(request.Name, request.Description, translations);
        menuGroup.UpdateIcon(request.Icon);
        menuGroup.UpdateSortOrder(request.SortOrder);

        if (request.IsActive)
            menuGroup.Activate();
        else
            menuGroup.Deactivate();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

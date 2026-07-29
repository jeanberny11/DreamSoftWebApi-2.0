using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Domain.Repositories;
using DreamSoft.Domain.ValueObjects;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.MenuOptions.UpdateMenuOption;

public class UpdateMenuOptionCommandHandler(
    IMenuOptionRepository menuOptionRepository,
    IModuleRepository moduleRepository,
    IMenuGroupRepository menuGroupRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateMenuOptionCommand, Unit>
{
    public async Task<Unit> Handle(
        UpdateMenuOptionCommand request,
        CancellationToken cancellationToken)
    {
        var menuOption = await menuOptionRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "MenuOption", request.Id);

        var moduleExists = await moduleRepository.AnyAsync(
            m => m.Id == request.ModuleId, cancellationToken);

        if (!moduleExists)
            throw new NotFoundException(ErrorMessageKeys.NotFound, "Module", request.ModuleId);

        var menuGroupExists = await menuGroupRepository.AnyAsync(
            mg => mg.Id == request.MenuGroupId, cancellationToken);

        if (!menuGroupExists)
            throw new NotFoundException(ErrorMessageKeys.NotFound, "MenuGroup", request.MenuGroupId);

        var es = request.Translations.Spanish;
        var en = request.Translations.English;

        var translations = TranslatedString.Create(
            BaseTranslatedProperties.Create(es.Name, es.Description),
            en is not null ? BaseTranslatedProperties.Create(en.Name, en.Description) : null);

        menuOption.UpdateDetails(request.Name, request.Description, translations);
        menuOption.MoveToModule(request.ModuleId);
        menuOption.MoveToMenuGroup(request.MenuGroupId);
        menuOption.UpdateRoute(request.Route);
        menuOption.UpdateIcon(request.Icon);
        menuOption.UpdateSortOrder(request.SortOrder);

        if (request.IsActive)
            menuOption.Activate();
        else
            menuOption.Deactivate();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Admin.MenuOptions.GetMenuOptions;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using DreamSoft.Domain.ValueObjects;
using MediatR;

namespace DreamSoft.Application.Features.Admin.MenuOptions.CreateMenuOption;

public class CreateMenuOptionCommandHandler(
    IMenuOptionRepository menuOptionRepository,
    IModuleRepository moduleRepository,
    IMenuGroupRepository menuGroupRepository,
    IRequestLanguageService languageService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateMenuOptionCommand, MenuOptionDto>
{
    public async Task<MenuOptionDto> Handle(
        CreateMenuOptionCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await menuOptionRepository.AnyAsync(
            mo => mo.Code == request.Code.ToUpper().Trim(), cancellationToken);

        if (exists)
            throw new ConflictException("MenuOptionCodeAlreadyExists", request.Code);

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

        var menuOption = MenuOption.Create(
            request.Code,
            request.Name,
            translations,
            request.ModuleId,
            request.MenuGroupId,
            request.Description,
            request.Route,
            request.Icon,
            request.SortOrder);

        await menuOptionRepository.AddAsync(menuOption, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var language = languageService.Resolve();

        return new MenuOptionDto(
            menuOption.Id,
            menuOption.Code,
            menuOption.Translations.GetNameOrFallback(language, menuOption.Name),
            menuOption.Translations.GetDescriptionOrFallback(language, menuOption.Description),
            menuOption.ModuleId,
            menuOption.MenuGroupId,
            menuOption.Route,
            menuOption.Icon,
            menuOption.SortOrder,
            menuOption.IsActive);
    }
}

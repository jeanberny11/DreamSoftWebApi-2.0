using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.AdminApp.MenuGroups.DTOs;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using DreamSoft.Domain.ValueObjects;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.MenuGroups.CreateMenuGroup;

public class CreateMenuGroupCommandHandler(
    IMenuGroupRepository menuGroupRepository,
    IRequestLanguageService languageService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateMenuGroupCommand, MenuGroupDto>
{
    public async Task<MenuGroupDto> Handle(
        CreateMenuGroupCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await menuGroupRepository.AnyAsync(
            mg => mg.Code == request.Code.ToUpper().Trim(), cancellationToken);

        if (exists)
            throw new ConflictException(ErrorMessageKeys.MenuGroupCodeAlreadyExists, request.Code);

        var es = request.Translations.Spanish;
        var en = request.Translations.English;

        var translations = TranslatedString.Create(
            BaseTranslatedProperties.Create(es.Name, es.Description),
            en is not null ? BaseTranslatedProperties.Create(en.Name, en.Description) : null);

        var menuGroup = MenuGroup.Create(
            request.Code,
            request.Name,
            translations,
            request.Description,
            request.Icon,
            request.SortOrder);

        await menuGroupRepository.AddAsync(menuGroup, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var language = languageService.Resolve();

        return new MenuGroupDto(
            menuGroup.Id,
            menuGroup.Code,
            menuGroup.Translations.GetNameOrFallback(language, menuGroup.Name),
            menuGroup.Translations.GetDescriptionOrFallback(language, menuGroup.Description),
            menuGroup.Icon,
            menuGroup.SortOrder,
            menuGroup.IsActive);
    }
}

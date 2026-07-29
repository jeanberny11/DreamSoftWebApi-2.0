using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.AdminApp.Modules.DTOs;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using DreamSoft.Domain.ValueObjects;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Modules.CreateModule;

public class CreateModuleCommandHandler(
    IModuleRepository moduleRepository,
    IRequestLanguageService languageService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateModuleCommand, ModuleDto>
{
    public async Task<ModuleDto> Handle(
        CreateModuleCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await moduleRepository.AnyAsync(
            m => m.Code == request.Code.ToUpper().Trim(), cancellationToken);

        if (exists)
            throw new ConflictException(ErrorMessageKeys.ModuleCodeAlreadyExists, request.Code);

        var es = request.Translations.Spanish;
        var en = request.Translations.English;

        var translations = TranslatedString.Create(
            BaseTranslatedProperties.Create(es.Name, es.Description),
            en is not null ? BaseTranslatedProperties.Create(en.Name, en.Description) : null);

        var module = Module.Create(
            request.Code,
            request.Name,
            translations,
            request.Description,
            request.Icon,
            request.SortOrder);

        await moduleRepository.AddAsync(module, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var language = languageService.Resolve();

        return new ModuleDto(
            module.Id,
            module.Code,
            module.Translations.GetNameOrFallback(language, module.Name),
            module.Translations.GetDescriptionOrFallback(language, module.Description),
            module.Icon,
            module.SortOrder,
            module.IsActive);
    }
}

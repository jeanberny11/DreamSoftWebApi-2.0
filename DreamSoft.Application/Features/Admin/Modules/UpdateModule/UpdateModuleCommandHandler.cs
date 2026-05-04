using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Admin.Modules.GetModules;
using DreamSoft.Domain.Repositories;
using DreamSoft.Domain.ValueObjects;
using MediatR;

namespace DreamSoft.Application.Features.Admin.Modules.UpdateModule;

public class UpdateModuleCommandHandler(
    IModuleRepository moduleRepository,
    IRequestLanguageService languageService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateModuleCommand, ModuleDto>
{
    public async Task<ModuleDto> Handle(
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

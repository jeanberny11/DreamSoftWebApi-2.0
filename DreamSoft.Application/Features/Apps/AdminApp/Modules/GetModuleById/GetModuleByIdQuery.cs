using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.AdminApp.Modules.DTOs;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Modules.GetModuleById;

public record GetModuleByIdQuery(int Id, string? Language = null) : IRequest<ModuleDto>;

public class GetModuleByIdQueryHandler(
    IModuleRepository moduleRepository,
    IRequestLanguageService languageService)
    : IRequestHandler<GetModuleByIdQuery, ModuleDto>
{
    public async Task<ModuleDto> Handle(
        GetModuleByIdQuery request,
        CancellationToken cancellationToken)
    {
        var language = languageService.Resolve(request.Language);
        var module   = await moduleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "Module", request.Id);

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

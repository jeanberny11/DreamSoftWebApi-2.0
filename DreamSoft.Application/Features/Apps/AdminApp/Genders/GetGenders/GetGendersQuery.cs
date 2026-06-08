using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.AdminApp.Genders.DTOs;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Genders.GetGenders;

public record GetGendersQuery(string? Language = null) : IRequest<IReadOnlyList<GenderDto>>;

public class GetGendersQueryHandler(
    IGenderRepository genderRepository,
    IRequestLanguageService languageService)
    : IRequestHandler<GetGendersQuery, IReadOnlyList<GenderDto>>
{
    public async Task<IReadOnlyList<GenderDto>> Handle(
        GetGendersQuery request,
        CancellationToken cancellationToken)
    {
        var language = languageService.Resolve(request.Language);
        var genders  = await genderRepository.GetAllAsync(cancellationToken);

        return genders
            .Select(g => new GenderDto(
                g.Id,
                g.Code,
                g.Translations.GetNameOrFallback(language, g.Name),
                g.IsActive))
            .ToList();
    }
}

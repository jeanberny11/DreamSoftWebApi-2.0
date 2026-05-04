using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Admin.Genders.GetGenders;

// ── Shared DTO ────────────────────────────────────────────────────────────────

public record GenderDto(int Id, string Code, string Name, bool IsActive);

// ── Get All (active + inactive) — SuperAdmin only ─────────────────────────────

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

// ── Get All Active — Public ───────────────────────────────────────────────────

public record GetActiveGendersQuery(string? Language = null) : IRequest<IReadOnlyList<GenderDto>>;

public class GetActiveGendersQueryHandler(
    IGenderRepository genderRepository,
    IRequestLanguageService languageService)
    : IRequestHandler<GetActiveGendersQuery, IReadOnlyList<GenderDto>>
{
    public async Task<IReadOnlyList<GenderDto>> Handle(
        GetActiveGendersQuery request,
        CancellationToken cancellationToken)
    {
        var language = languageService.Resolve(request.Language);
        var genders  = await genderRepository.GetAllActiveAsync(cancellationToken);

        return genders
            .Select(g => new GenderDto(
                g.Id,
                g.Code,
                g.Translations.GetNameOrFallback(language, g.Name),
                g.IsActive))
            .ToList();
    }
}

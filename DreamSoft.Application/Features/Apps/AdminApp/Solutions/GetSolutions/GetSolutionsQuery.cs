using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.AdminApp.Solutions.DTOs;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Solutions.GetSolutions;

public record GetSolutionsQuery(string? Language = null) : IRequest<IReadOnlyList<SolutionDto>>;

public class GetSolutionsQueryHandler(
    ISolutionRepository solutionRepository,
    IRequestLanguageService languageService)
    : IRequestHandler<GetSolutionsQuery, IReadOnlyList<SolutionDto>>
{
    public async Task<IReadOnlyList<SolutionDto>> Handle(
        GetSolutionsQuery request,
        CancellationToken cancellationToken)
    {
        var language  = languageService.Resolve(request.Language);
        var solutions = await solutionRepository.GetAllAsync(cancellationToken);

        return solutions
            .Select(s => new SolutionDto(
                s.Id, s.Code,
                s.Translations.GetNameOrFallback(language, s.Name),
                s.Translations.GetDescriptionOrFallback(language, s.Description),
                s.Icon, s.SortOrder, s.IsActive))
            .ToList();
    }
}

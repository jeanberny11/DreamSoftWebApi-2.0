using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Admin.Solutions.GetSolutions;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Admin.Solutions.GetSolutionById;

public record GetSolutionByIdQuery(int Id, string? Language = null) : IRequest<SolutionDto>;

public class GetSolutionByIdQueryHandler(
    ISolutionRepository solutionRepository,
    IRequestLanguageService languageService)
    : IRequestHandler<GetSolutionByIdQuery, SolutionDto>
{
    public async Task<SolutionDto> Handle(
        GetSolutionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var language = languageService.Resolve(request.Language);
        var solution = await solutionRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "Solution", request.Id);

        return new SolutionDto(
            solution.Id,
            solution.Code,
            solution.Translations.GetNameOrFallback(language, solution.Name),
            solution.Translations.GetDescriptionOrFallback(language, solution.Description),
            solution.Icon,
            solution.SortOrder,
            solution.IsActive);
    }
}

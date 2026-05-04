using DreamSoft.Application.Features.Admin.Solutions.GetSolutions;
using DreamSoft.Application.Features.Admin.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Admin.Solutions.UpdateSolution;

public record UpdateSolutionCommand(
    int             Id,
    string          Name,
    string?         Description,
    string?         Icon,
    int             SortOrder,
    TranslationsDto Translations,
    bool            IsActive) : IRequest<SolutionDto>;

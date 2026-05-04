using DreamSoft.Application.Features.Admin.Solutions.GetSolutions;
using DreamSoft.Application.Features.Admin.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Admin.Solutions.CreateSolution;

public record CreateSolutionCommand(
    string          Code,
    string          Name,
    TranslationsDto Translations,
    string?         Description = null,
    string?         Icon        = null,
    int             SortOrder   = 0) : IRequest<SolutionDto>;

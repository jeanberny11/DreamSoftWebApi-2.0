using DreamSoft.Application.Features.Apps.AdminApp.Solutions.GetSolutions;
using DreamSoft.Application.Features.Apps.AdminApp.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Solutions.CreateSolution;

public record CreateSolutionCommand(
    string          Code,
    string          Name,
    TranslationsDto Translations,
    string?         Description = null,
    string?         Icon        = null,
    int             SortOrder   = 0) : IRequest<SolutionDto>;

using DreamSoft.Application.Features.Apps.AdminApp.Solutions.GetSolutions;
using DreamSoft.Application.Features.Apps.AdminApp.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Solutions.UpdateSolution;

public record UpdateSolutionCommand(
    int             Id,
    string          Name,
    string?         Description,
    string?         Icon,
    int             SortOrder,
    TranslationsDto Translations,
    bool            IsActive) : IRequest<SolutionDto>;

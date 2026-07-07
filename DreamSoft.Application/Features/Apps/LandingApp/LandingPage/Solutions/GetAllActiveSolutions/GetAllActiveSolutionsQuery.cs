namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Solutions.GetAllActiveSolutions;

using MediatR;

public record GetAllActiveSolutionsQuery(string? Language = null) : IRequest<List<SolutionResponse>>;

public record SolutionResponse(
    int SolutionId,
    string Code,
    string Name,
    string Description,
    string Icon
);
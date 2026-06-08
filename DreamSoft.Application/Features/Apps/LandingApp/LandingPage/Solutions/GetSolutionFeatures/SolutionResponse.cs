namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Solutions.GetSolutionFeatures;

public record SolutionFeature(
    string Code,
    string Name,
    string Description,
    string IconUrl
);

public record SolutionResponse(
    string Code,
    string Name,
    string Description,
    string IconUrl,
    List<SolutionFeature> Features
);

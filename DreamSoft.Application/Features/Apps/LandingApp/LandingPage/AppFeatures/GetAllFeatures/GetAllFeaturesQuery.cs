namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.AppFeatures.GetAllFeatures;
using MediatR;

public record FeatureOption(
    string Code,
    string Name,
    string Description,
    string Icon,
    int SortOrder
);

public record FeatureGroup(
    string Code,
    string Name,
    string Description,
    string Icon,
    int SortOrder,
    List<FeatureOption> Options
);

public record Feature(
    string Code,
    string Name,
    string Description,
    string Icon,
    int SortOrder,
    List<FeatureGroup> Groups
);

public record GetAllFeaturesQuery(string? Language = null) : IRequest<List<Feature>>;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.AppFeatures.GetAppFeaturesQuery;

public record GetAppFeaturesResponse(
    string Code,
    string Name,
    string Description,
    string Icon,
    int SortOrder,
    List<AppFeatureOptionDto> Modules
);

public record AppFeatureOptionDto(
    string Code,
    string Name,
    string Description,
    string Icon,
    int SortOrder
);

    public record GetAppFeaturesQuery(string? Language = null) : IRequest<List<GetAppFeaturesResponse>>;

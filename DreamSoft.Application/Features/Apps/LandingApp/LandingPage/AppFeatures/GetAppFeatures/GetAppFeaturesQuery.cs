using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.AppFeatures.GetAppFeatures;

public record GetAppFeaturesResponse(
    string Code,
    string Name,
    string Description,
    string Icon,
    int SortOrder,
    List<AppFeatureOptionDto> Options
);

public record AppFeatureOptionDto(
    string Code,
    string Name,
    string Description,
    string Icon,
    int SortOrder
);

public record GetAppFeaturesQuery(string? Language = null) : IRequest<List<GetAppFeaturesResponse>>;
namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.AppFeatures.GetAllFeatures;

using MediatR;
using DreamSoft.Domain.Repositories;

public class GetAllFeaturesQueryHandler(IMenuOptionRepository menuOptionRepository)
    : IRequestHandler<GetAllFeaturesQuery, List<Feature>>
{
    public async Task<List<Feature>> Handle(
        GetAllFeaturesQuery request,
        CancellationToken cancellationToken)
    {
        var language = request.Language ?? "es";

        var menuOptions = await menuOptionRepository.GetAllActiveWithModuleAndGroupAsync(cancellationToken);
        var features = menuOptions
            .GroupBy(mo => mo.Module)
            .Select(moduleGroup => new Feature(
                Code: moduleGroup.Key.Code,
                Name: moduleGroup.Key.Translations.GetNameOrFallback(language, moduleGroup.Key.Name),
                Description: moduleGroup.Key.Translations.GetDescriptionOrFallback(language, moduleGroup.Key.Description),
                Icon: moduleGroup.Key.Icon,
                SortOrder: moduleGroup.Key.SortOrder,
                Groups: [.. moduleGroup
                    .GroupBy(mo => mo.MenuGroup)
                    .Select(group => new FeatureGroup(
                        Code: group.Key.Code,
                        Name: group.Key.Translations.GetNameOrFallback(language, group.Key.Name),
                        Description: group.Key.Translations.GetDescriptionOrFallback(language, group.Key.Description),
                        Icon: group.Key.Icon,
                        SortOrder: group.Key.SortOrder,
                        Options: [.. group
                            .Select(option => new FeatureOption(
                                Code: option.Code,
                                Name: option.Translations.GetNameOrFallback(language, option.Name),
                                Description: option.Translations.GetDescriptionOrFallback(language, option.Description),
                                Icon: option.Icon,
                                SortOrder: option.SortOrder
                            ))
                            .OrderBy(o => o.SortOrder)]
                    ))
                    .OrderBy(g => g.SortOrder)]
            ))
            .OrderBy(f => f.SortOrder)
            .ToList();
        return features;
    }
}
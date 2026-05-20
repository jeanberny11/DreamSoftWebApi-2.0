using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.AppFeatures.GetAppFeaturesQuery;

public class GetAppFeaturesQueryHandler(IModuleRepository moduleRepository)
    : IRequestHandler<GetAppFeaturesQuery, List<GetAppFeaturesResponse>>
{
    public async Task<List<GetAppFeaturesResponse>> Handle(
        GetAppFeaturesQuery request,
        CancellationToken cancellationToken)
    {
        var language = request.Language ?? "es";

        var modules = await moduleRepository.GetAllActiveWithMenuOptionsAsync(cancellationToken);

        return [.. modules
            .Select(module => new GetAppFeaturesResponse(
                module.Code,
                module.Translations.GetNameOrFallback(language,module.Name),
                module.Translations.GetDescriptionOrFallback(language,module.Description),
                module.Icon,
                module.SortOrder,
                [.. module.MenuOptions
                    .OrderBy(mo => mo.SortOrder)
                    .Select(mo => new AppFeatureOptionDto(
                        mo.Code,
                        mo.Translations.GetNameOrFallback(language, mo.Name),
                        mo.Translations.GetDescriptionOrFallback(language, mo.Description),
                        mo.Icon,
                        mo.SortOrder))]
            ))];
    }
}

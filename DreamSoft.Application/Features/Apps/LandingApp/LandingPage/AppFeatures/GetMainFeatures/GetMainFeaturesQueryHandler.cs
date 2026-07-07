using DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Dtos;
using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.AppFeatures.GetMainFeatures;

public class GetMainFeaturesQueryHandler(IModuleRepository moduleRepository)
    : IRequestHandler<GetMainFeaturesQuery, List<ModuleDto>>
{
    public async Task<List<ModuleDto>> Handle(
        GetMainFeaturesQuery request,
        CancellationToken cancellationToken)
    {
        var language = request.Language ?? "es";

        var modules = await moduleRepository.GetActiveByCodesWithMenuOptionsAsync(
            FeatureCodes.TopFeaturesCodes, cancellationToken);

        return [.. modules.Select(module => module.ToDto(language))];
    }
}

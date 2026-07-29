using DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Dtos;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.AppFeatures.GetAllFeatures;

public class GetAllFeaturesQueryHandler(IModuleRepository moduleRepository)
    : IRequestHandler<GetAllFeaturesQuery, List<ModuleDto>>
{
    public async Task<List<ModuleDto>> Handle(
        GetAllFeaturesQuery request,
        CancellationToken cancellationToken)
    {
        var language = request.Language ?? "es";

        var modules = await moduleRepository.GetAllActiveWithMenuOptionsAndGroupsAsync(cancellationToken);

        return [.. modules.Select(module => module.ToDto(language))];
    }
}

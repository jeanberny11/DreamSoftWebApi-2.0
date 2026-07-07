namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Solutions.GetSolutionFeatures;

using DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Dtos;
using DreamSoft.Domain.Repositories;
using MediatR;

public class SolutionQueryHandler(ISolutionRepository solutionRepository)
    : IRequestHandler<SolutionQuery, List<SolutionDto>>
{
    public async Task<List<SolutionDto>> Handle(
        SolutionQuery request,
        CancellationToken cancellationToken)
    {
        var language = request.Language ?? "es";

        var solutions = await solutionRepository.GetAllActiveWithPlansAndFeaturesAsync(cancellationToken);

        return [.. solutions.Select(solution => solution.ToDto(language))];
    }
}

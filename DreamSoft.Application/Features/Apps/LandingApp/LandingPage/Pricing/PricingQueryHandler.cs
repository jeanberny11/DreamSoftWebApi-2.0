using DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Dtos;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Pricing;

public class PricingQueryHandler(ISolutionRepository solutionRepository)
    : IRequestHandler<PricingQuery, List<SolutionDto>>
{
    public async Task<List<SolutionDto>> Handle(
        PricingQuery request,
        CancellationToken cancellationToken)
    {
        var language = request.Language ?? "es";

        var solutions = await solutionRepository.GetAllActiveWithPlansAsync(cancellationToken);

        return [.. solutions.Select(solution => solution.ToDto(language))];
    }
}

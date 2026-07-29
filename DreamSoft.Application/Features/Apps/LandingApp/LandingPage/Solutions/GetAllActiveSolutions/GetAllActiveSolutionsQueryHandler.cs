using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Solutions.GetAllActiveSolutions;

public class GetAllActiveSolutionsQueryHandler(ISolutionRepository solutionRepository) : IRequestHandler<GetAllActiveSolutionsQuery, List<SolutionResponse>>
{
    public async Task<List<SolutionResponse>> Handle(GetAllActiveSolutionsQuery request, CancellationToken cancellationToken)
    {
        var language = request.Language ?? "es";

        var solutions = await solutionRepository.GetAllActiveAsync(cancellationToken);

        return [.. solutions.Select(s => new SolutionResponse(
            s.Id,
            s.Code,
            s.Name,
            s.Description,
            s.Icon
        ))];
    }
}
namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Solutions.GetSolutionByCode;

using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Dtos;
using DreamSoft.Domain.Repositories;
using MediatR;

public class GetSolutionByCodeQueryHandler(ISolutionRepository solutionRepository)
    : IRequestHandler<GetSolutionByCodeQuery, SolutionDto>
{
    public async Task<SolutionDto> Handle(
        GetSolutionByCodeQuery request,
        CancellationToken cancellationToken)
    {
        var language = request.Language ?? "es";

        var solution = await solutionRepository.GetActiveByCodeWithPlansAndFeaturesAsync(request.Code, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "Solution", request.Code);

        return solution.ToDto(language);
    }
}

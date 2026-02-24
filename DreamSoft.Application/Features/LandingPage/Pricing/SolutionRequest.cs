using MediatR;

namespace DreamSoft.Application.Features.LandingPage.Pricing;

public class SolutionRequest : IRequest<List<SolutionDTO>>
{
    public string Language { get; set; } = null!;
}
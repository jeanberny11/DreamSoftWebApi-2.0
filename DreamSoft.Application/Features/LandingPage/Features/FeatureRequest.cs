using MediatR;

namespace DreamSoft.Application.Features.LandingPage.Features;

public class FeatureRequest : IRequest<List<FeatureResponse>>
{
    public string Language { get; set; } = null!;
}
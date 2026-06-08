namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Solutions.GetSolutionFeatures;

using MediatR;

  public record SolutionQuery(string? Language = null) : IRequest<List<SolutionResponse>>;

namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Dtos;

public record SolutionDto(
    int SolutionId,
    string Code,
    string Name,
    string Description,
    string Icon,
    List<SubscriptionPlanDto> Plans
);
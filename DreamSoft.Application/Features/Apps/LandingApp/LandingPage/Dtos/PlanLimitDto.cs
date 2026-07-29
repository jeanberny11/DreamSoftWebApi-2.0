namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Dtos;

public record PlanLimitDto(
    int PlanLimitId,
    string LimitKey,
    int LimitValue,
    string Description
);
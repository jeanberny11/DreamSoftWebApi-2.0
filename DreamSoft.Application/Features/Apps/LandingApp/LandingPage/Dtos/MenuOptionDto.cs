namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Dtos;
public record MenuOptionDto(
    int MenuOptionId,
    string Code,
    string Name,
    string Description,
    string Icon,
    int SortOrder
);
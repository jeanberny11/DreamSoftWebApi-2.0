namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Dtos;

public record MenuGroupDto(
    int MenuGroupId,
    string Code,
    string Name,
    string Description,
    string Icon,
    int SortOrder,
    List<MenuOptionDto> Options
);
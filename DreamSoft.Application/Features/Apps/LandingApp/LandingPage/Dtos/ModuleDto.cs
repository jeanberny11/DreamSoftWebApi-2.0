namespace DreamSoft.Application.Features.Apps.LandingApp.LandingPage.Dtos;

public record ModuleDto(
    int ModuleId,
    string Code,
    string Name,
    string Description,
    string Icon,
    int SortOrder,
    List<MenuGroupDto> Groups
);
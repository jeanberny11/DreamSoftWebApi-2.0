namespace DreamSoft.Application.Features.Apps.AdminApp.MenuGroups.DTOs;

public record MenuGroupDto(
    int Id,
    string Code,
    string Name,
    string Description,
    string Icon,
    int SortOrder,
    bool IsActive);

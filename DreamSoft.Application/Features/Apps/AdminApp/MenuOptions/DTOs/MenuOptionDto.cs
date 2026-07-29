namespace DreamSoft.Application.Features.Apps.AdminApp.MenuOptions.DTOs;

public record MenuOptionDto(
    int Id,
    string Code,
    string Name,
    string Description,
    int ModuleId,
    int MenuGroupId,
    string Route,
    string Icon,
    int SortOrder,
    bool IsActive);

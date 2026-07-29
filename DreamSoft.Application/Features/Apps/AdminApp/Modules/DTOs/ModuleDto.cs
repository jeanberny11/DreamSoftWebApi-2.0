namespace DreamSoft.Application.Features.Apps.AdminApp.Modules.DTOs;

public record ModuleDto(
    int Id,
    string Code,
    string Name,
    string Description,
    string Icon,
    int SortOrder,
    bool IsActive);

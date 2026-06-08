namespace DreamSoft.Application.Features.Apps.AdminApp.Solutions.DTOs;

public record SolutionDto(
    int Id,
    string Code,
    string Name,
    string? Description,
    string? Icon,
    int SortOrder,
    bool IsActive);

namespace DreamSoft.Application.Features.Apps.AdminApp.Shared;

/// <summary>
/// Represents a single language translation entry.
/// </summary>
public record TranslationPropDto(
    string Name,
    string? Description = null);

/// <summary>
/// Flexible translation input DTO used across all admin create/update commands.
/// Adding a new language only requires adding a new property here —
/// no endpoint or command signature changes needed.
/// </summary>
public record TranslationsDto(
    TranslationPropDto Spanish,
    TranslationPropDto? English = null);

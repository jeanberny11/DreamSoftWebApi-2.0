namespace DreamSoft.Application.Features.Apps.AdminApp.Currencies.DTOs;

public record CurrencyDto(
    int Id,
    string Code,
    string Name,
    string NativeName,
    bool IsDefault,
    bool IsActive);

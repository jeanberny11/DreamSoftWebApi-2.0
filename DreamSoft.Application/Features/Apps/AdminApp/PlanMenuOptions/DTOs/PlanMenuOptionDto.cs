namespace DreamSoft.Application.Features.Apps.AdminApp.PlanMenuOptions.DTOs;

public record PlanMenuOptionDto(
    int PlanId,
    int MenuOptionId,
    string MenuOptionCode,
    string MenuOptionName);

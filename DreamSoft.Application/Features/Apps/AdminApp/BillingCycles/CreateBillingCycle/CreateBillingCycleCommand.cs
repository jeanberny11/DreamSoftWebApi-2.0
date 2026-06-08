using DreamSoft.Application.Features.Apps.AdminApp.BillingCycles.DTOs;
using DreamSoft.Application.Features.Apps.AdminApp.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.BillingCycles.CreateBillingCycle;

public record CreateBillingCycleCommand(
    string Code,
    string Name,
    string? Description,
    TranslationsDto Translations,
    int Months) : IRequest<BillingCycleDto>;

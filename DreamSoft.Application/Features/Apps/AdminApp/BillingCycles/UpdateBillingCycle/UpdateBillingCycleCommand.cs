using DreamSoft.Application.Features.Apps.AdminApp.BillingCycles.GetBillingCycles;
using DreamSoft.Application.Features.Apps.AdminApp.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.BillingCycles.UpdateBillingCycle;

public record UpdateBillingCycleCommand(
    int             Id,
    string          Name,
    string?         Description,
    TranslationsDto Translations,
    bool            IsActive) : IRequest<BillingCycleDto>;

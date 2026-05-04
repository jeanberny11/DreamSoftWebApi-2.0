using DreamSoft.Application.Features.Admin.BillingCycles.GetBillingCycles;
using DreamSoft.Application.Features.Admin.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Admin.BillingCycles.UpdateBillingCycle;

public record UpdateBillingCycleCommand(
    int             Id,
    string          Name,
    string?         Description,
    TranslationsDto Translations,
    bool            IsActive) : IRequest<BillingCycleDto>;

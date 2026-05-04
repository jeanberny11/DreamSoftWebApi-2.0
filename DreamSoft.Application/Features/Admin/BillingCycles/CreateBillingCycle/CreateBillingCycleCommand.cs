using DreamSoft.Application.Features.Admin.BillingCycles.GetBillingCycles;
using DreamSoft.Application.Features.Admin.Shared;
using MediatR;

namespace DreamSoft.Application.Features.Admin.BillingCycles.CreateBillingCycle;

public record CreateBillingCycleCommand(
    string          Code,
    string          Name,
    string?         Description,
    TranslationsDto Translations,
    int             Months) : IRequest<BillingCycleDto>;

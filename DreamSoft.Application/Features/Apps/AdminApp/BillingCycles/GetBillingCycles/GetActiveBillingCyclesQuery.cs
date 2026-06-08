using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.AdminApp.BillingCycles.DTOs;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.BillingCycles.GetBillingCycles;

public record GetActiveBillingCyclesQuery(string? Language = null) : IRequest<IReadOnlyList<BillingCycleDto>>;

public class GetActiveBillingCyclesQueryHandler(
    IBillingCycleRepository billingCycleRepository,
    IRequestLanguageService languageService)
    : IRequestHandler<GetActiveBillingCyclesQuery, IReadOnlyList<BillingCycleDto>>
{
    public async Task<IReadOnlyList<BillingCycleDto>> Handle(
        GetActiveBillingCyclesQuery request, CancellationToken cancellationToken)
    {
        var language = languageService.Resolve(request.Language);
        var cycles   = await billingCycleRepository.GetAllActiveAsync(cancellationToken);

        return cycles
            .Select(c => new BillingCycleDto(
                c.Id, c.Code,
                c.Translations.GetNameOrFallback(language, c.Name),
                c.Translations.GetDescriptionOrFallback(language, c.Description),
                c.Months, c.IsActive))
            .ToList();
    }
}

using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Admin.BillingCycles.GetBillingCycles;

// ── Shared DTO ────────────────────────────────────────────────────────────────

public record BillingCycleDto(
    int    Id,
    string Code,
    string Name,
    string Description,
    int    Months,
    bool   IsActive);

// ── Get All ───────────────────────────────────────────────────────────────────

public record GetBillingCyclesQuery(string? Language = null) : IRequest<IReadOnlyList<BillingCycleDto>>;

public class GetBillingCyclesQueryHandler(
    IBillingCycleRepository billingCycleRepository,
    IRequestLanguageService languageService)
    : IRequestHandler<GetBillingCyclesQuery, IReadOnlyList<BillingCycleDto>>
{
    public async Task<IReadOnlyList<BillingCycleDto>> Handle(
        GetBillingCyclesQuery request, CancellationToken cancellationToken)
    {
        var language = languageService.Resolve(request.Language);
        var cycles = await billingCycleRepository.GetAllAsync(cancellationToken);

        return cycles
            .Select(c => new BillingCycleDto(
                c.Id, c.Code,
                c.Translations.GetNameOrFallback(language, c.Name),
                c.Translations.GetDescriptionOrFallback(language, c.Description),
                c.Months, c.IsActive))
            .ToList();
    }
}

// ── Get By Id ─────────────────────────────────────────────────────────────────

public record GetBillingCycleByIdQuery(int Id, string? Language = null) : IRequest<BillingCycleDto>;

public class GetBillingCycleByIdQueryHandler(
    IBillingCycleRepository billingCycleRepository,
    IRequestLanguageService languageService)
    : IRequestHandler<GetBillingCycleByIdQuery, BillingCycleDto>
{
    public async Task<BillingCycleDto> Handle(
        GetBillingCycleByIdQuery request, CancellationToken cancellationToken)
    {
        var language = languageService.Resolve(request.Language);
        var cycle = await billingCycleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "BillingCycle", request.Id);

        return new BillingCycleDto(
            cycle.Id, cycle.Code,
            cycle.Translations.GetNameOrFallback(language, cycle.Name),
            cycle.Translations.GetDescriptionOrFallback(language, cycle.Description),
            cycle.Months, cycle.IsActive);
    }
}

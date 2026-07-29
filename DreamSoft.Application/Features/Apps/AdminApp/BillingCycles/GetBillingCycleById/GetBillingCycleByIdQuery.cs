using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.AdminApp.BillingCycles.DTOs;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.BillingCycles.GetBillingCycleById;

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
        var cycle    = await billingCycleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "BillingCycle", request.Id);

        return new BillingCycleDto(
            cycle.Id, cycle.Code,
            cycle.Translations.GetNameOrFallback(language, cycle.Name),
            cycle.Translations.GetDescriptionOrFallback(language, cycle.Description),
            cycle.Months, cycle.IsActive);
    }
}

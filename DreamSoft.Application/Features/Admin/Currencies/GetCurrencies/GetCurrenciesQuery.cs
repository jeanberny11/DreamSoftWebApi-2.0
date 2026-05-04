using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Admin.Currencies.GetCurrencies;

// ── Shared DTO ────────────────────────────────────────────────────────────────

public record CurrencyDto(
    int Id,
    string Code,
    string Name,
    string NativeName,
    bool IsDefault,
    bool IsActive);

// ── Get All (active + inactive) — SuperAdmin only ─────────────────────────────

public record GetCurrenciesQuery : IRequest<IReadOnlyList<CurrencyDto>>;

public class GetCurrenciesQueryHandler(ICurrencyRepository currencyRepository)
    : IRequestHandler<GetCurrenciesQuery, IReadOnlyList<CurrencyDto>>
{
    public async Task<IReadOnlyList<CurrencyDto>> Handle(
        GetCurrenciesQuery request,
        CancellationToken cancellationToken)
    {
        var currencies = await currencyRepository.GetAllAsync(cancellationToken);

        return currencies
            .Select(c => new CurrencyDto(
                c.Id, c.Code, c.Name, c.NativeName, c.IsDefault, c.IsActive))
            .ToList();
    }
}

// ── Get All Active — Public ───────────────────────────────────────────────────

public record GetActiveCurrenciesQuery : IRequest<IReadOnlyList<CurrencyDto>>;

public class GetActiveCurrenciesQueryHandler(ICurrencyRepository currencyRepository)
    : IRequestHandler<GetActiveCurrenciesQuery, IReadOnlyList<CurrencyDto>>
{
    public async Task<IReadOnlyList<CurrencyDto>> Handle(
        GetActiveCurrenciesQuery request,
        CancellationToken cancellationToken)
    {
        var currencies = await currencyRepository.GetAllActiveAsync(cancellationToken);

        return currencies
            .Select(c => new CurrencyDto(
                c.Id, c.Code, c.Name, c.NativeName, c.IsDefault, c.IsActive))
            .ToList();
    }
}

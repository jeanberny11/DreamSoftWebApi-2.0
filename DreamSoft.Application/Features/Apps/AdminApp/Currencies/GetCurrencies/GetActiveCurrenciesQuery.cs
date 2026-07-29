using DreamSoft.Application.Features.Apps.AdminApp.Currencies.DTOs;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Currencies.GetCurrencies;

public record GetActiveCurrenciesQuery : IRequest<IReadOnlyList<CurrencyDto>>;

public class GetActiveCurrenciesQueryHandler(ICurrencyRepository currencyRepository)
    : IRequestHandler<GetActiveCurrenciesQuery, IReadOnlyList<CurrencyDto>>
{
    public async Task<IReadOnlyList<CurrencyDto>> Handle(
        GetActiveCurrenciesQuery request,
        CancellationToken cancellationToken)
    {
        var currencies = await currencyRepository.GetAllActiveAsync(cancellationToken);

        return [.. currencies
            .Select(c => new CurrencyDto(
                c.Id, c.Code, c.Name, c.NativeName, c.IsDefault, c.IsActive))];
    }
}

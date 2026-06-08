using DreamSoft.Application.Features.Apps.AdminApp.Currencies.DTOs;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Currencies.GetCurrencies;

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

using DreamSoft.Application.Features.Admin.Currencies.GetCurrencies;
using MediatR;

namespace DreamSoft.Application.Features.Admin.Currencies.CreateCurrency;

public record CreateCurrencyCommand(
    string Code,
    string Name,
    string NativeName,
    bool IsDefault = false) : IRequest<CurrencyDto>;

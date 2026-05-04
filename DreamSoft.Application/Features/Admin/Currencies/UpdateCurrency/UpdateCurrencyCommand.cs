using DreamSoft.Application.Features.Admin.Currencies.GetCurrencies;
using MediatR;

namespace DreamSoft.Application.Features.Admin.Currencies.UpdateCurrency;

public record UpdateCurrencyCommand(
    int Id,
    string Name,
    string NativeName,
    bool IsDefault,
    bool IsActive) : IRequest<CurrencyDto>;

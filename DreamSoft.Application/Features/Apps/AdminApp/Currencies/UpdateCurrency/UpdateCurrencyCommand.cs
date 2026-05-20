using DreamSoft.Application.Features.Apps.AdminApp.Currencies.GetCurrencies;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Currencies.UpdateCurrency;

public record UpdateCurrencyCommand(
    int Id,
    string Name,
    string NativeName,
    bool IsDefault,
    bool IsActive) : IRequest<CurrencyDto>;

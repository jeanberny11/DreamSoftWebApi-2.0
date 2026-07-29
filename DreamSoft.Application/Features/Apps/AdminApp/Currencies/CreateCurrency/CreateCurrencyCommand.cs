using DreamSoft.Application.Features.Apps.AdminApp.Currencies.DTOs;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Currencies.CreateCurrency;

public record CreateCurrencyCommand(
    string Code,
    string Name,
    string NativeName,
    bool IsDefault = false) : IRequest<CurrencyDto>;

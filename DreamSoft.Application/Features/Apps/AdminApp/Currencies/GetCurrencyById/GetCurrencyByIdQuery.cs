using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Features.Apps.AdminApp.Currencies.DTOs;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Currencies.GetCurrencyById;

public record GetCurrencyByIdQuery(int Id) : IRequest<CurrencyDto>;

public class GetCurrencyByIdQueryHandler(ICurrencyRepository currencyRepository)
    : IRequestHandler<GetCurrencyByIdQuery, CurrencyDto>
{
    public async Task<CurrencyDto> Handle(
        GetCurrencyByIdQuery request,
        CancellationToken cancellationToken)
    {
        var currency = await currencyRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "Currency", request.Id);

        return new CurrencyDto(
            currency.Id,
            currency.Code,
            currency.Name,
            currency.NativeName,
            currency.IsDefault,
            currency.IsActive);
    }
}

using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Features.Apps.AdminApp.Currencies.GetCurrencies;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Currencies.UpdateCurrency;

public class UpdateCurrencyCommandHandler(
    ICurrencyRepository currencyRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCurrencyCommand, CurrencyDto>
{
    public async Task<CurrencyDto> Handle(
        UpdateCurrencyCommand request,
        CancellationToken cancellationToken)
    {
        var currency = await currencyRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "Currency", request.Id);

        currency.UpdateDetails(
            request.Name,
            request.NativeName,
            request.IsDefault,
            request.IsActive);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CurrencyDto(
            currency.Id,
            currency.Code,
            currency.Name,
            currency.NativeName,
            currency.IsDefault,
            currency.IsActive);
    }
}

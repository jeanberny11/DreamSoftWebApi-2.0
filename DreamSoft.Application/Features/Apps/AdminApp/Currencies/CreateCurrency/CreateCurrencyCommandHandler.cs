using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Features.Apps.AdminApp.Currencies.DTOs;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Currencies.CreateCurrency;

public class CreateCurrencyCommandHandler(
    ICurrencyRepository currencyRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCurrencyCommand, CurrencyDto>
{
    public async Task<CurrencyDto> Handle(
        CreateCurrencyCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await currencyRepository.AnyAsync(
            c => c.Code == request.Code.ToUpper().Trim(), cancellationToken);

        if (exists)
            throw new ConflictException(ErrorMessageKeys.CurrencyCodeAlreadyExists, request.Code);

        var currency = Currency.Create(
            request.Code,
            request.Name,
            request.NativeName,
            request.IsDefault);

        await currencyRepository.AddAsync(currency, cancellationToken);
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

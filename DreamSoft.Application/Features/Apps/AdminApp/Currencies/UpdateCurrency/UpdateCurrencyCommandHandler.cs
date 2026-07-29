using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.Currencies.UpdateCurrency;

public class UpdateCurrencyCommandHandler(
    ICurrencyRepository currencyRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCurrencyCommand, Unit>
{
    public async Task<Unit> Handle(
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

        return Unit.Value;
    }
}

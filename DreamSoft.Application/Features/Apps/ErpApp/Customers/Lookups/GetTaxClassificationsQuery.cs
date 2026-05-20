using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.ErpApp.Customers.Lookups;

public record GetTaxClassificationsQuery : IRequest<IReadOnlyList<TaxClassificationDto>>;

public record TaxClassificationDto(
    int Id,
    string Code,
    string Name,
    string? NcfType,
    bool RequiresRnc);

public class GetTaxClassificationsQueryHandler(ITaxClassificationRepository taxClassificationRepository)
    : IRequestHandler<GetTaxClassificationsQuery, IReadOnlyList<TaxClassificationDto>>
{
    public async Task<IReadOnlyList<TaxClassificationDto>> Handle(
        GetTaxClassificationsQuery request,
        CancellationToken cancellationToken)
    {
        var classifications = await taxClassificationRepository.GetAllActiveAsync(cancellationToken);

        return classifications
            .Select(tc => new TaxClassificationDto(tc.Id, tc.Code, tc.Name, tc.NcfType, tc.RequiresRnc))
            .ToList();
    }
}

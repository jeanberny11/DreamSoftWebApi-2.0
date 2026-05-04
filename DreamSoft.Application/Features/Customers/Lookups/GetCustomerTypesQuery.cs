using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Customers.Lookups;

public record GetCustomerTypesQuery : IRequest<IReadOnlyList<LookupDto>>;

public class GetCustomerTypesQueryHandler(ICustomerTypeRepository customerTypeRepository)
    : IRequestHandler<GetCustomerTypesQuery, IReadOnlyList<LookupDto>>
{
    public async Task<IReadOnlyList<LookupDto>> Handle(
        GetCustomerTypesQuery request,
        CancellationToken cancellationToken)
    {
        var types = await customerTypeRepository.GetAllActiveAsync(cancellationToken);

        return types
            .Select(t => new LookupDto(t.Id, t.Code, t.Name, t.Description))
            .ToList();
    }
}

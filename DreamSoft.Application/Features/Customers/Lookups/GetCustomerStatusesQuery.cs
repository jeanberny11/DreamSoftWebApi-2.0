using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Customers.Lookups;

public record GetCustomerStatusesQuery : IRequest<IReadOnlyList<LookupDto>>;

public record LookupDto(int Id, string Code, string Name, string? Description);

public class GetCustomerStatusesQueryHandler(ICustomerStatusRepository customerStatusRepository)
    : IRequestHandler<GetCustomerStatusesQuery, IReadOnlyList<LookupDto>>
{
    public async Task<IReadOnlyList<LookupDto>> Handle(
        GetCustomerStatusesQuery request,
        CancellationToken cancellationToken)
    {
        var statuses = await customerStatusRepository.GetAllActiveAsync(cancellationToken);

        return statuses
            .Select(s => new LookupDto(s.Id, s.Code, s.Name, s.Description))
            .ToList();
    }
}

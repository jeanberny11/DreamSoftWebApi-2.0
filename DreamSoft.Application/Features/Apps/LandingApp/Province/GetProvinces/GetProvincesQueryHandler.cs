using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Province.GetProvinces;

public class GetProvincesQueryHandler(IProvinceRepository provinceRepository)
    : IRequestHandler<GetProvincesQuery, List<GetProvinceResponse>>
{
    public async Task<List<GetProvinceResponse>> Handle(
        GetProvincesQuery request,
        CancellationToken cancellationToken)
    {
        var provinces = await provinceRepository.FindAsync(p => p.IsActive, cancellationToken);
        return [.. provinces.Select(p => new GetProvinceResponse(
            ProvinceId: p.Id,
            Code: p.Code,
            Name: p.Name,
            CountryId: p.CountryId
        ))];
    }
}

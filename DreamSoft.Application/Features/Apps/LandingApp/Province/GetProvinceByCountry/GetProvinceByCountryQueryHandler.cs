using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Province.GetProvinceByCountry;

public class GetProvinceByCountryQueryHandler(IProvinceRepository provinceRepository)
    : IRequestHandler<GetProvinceByCountryQuery, List<GetProvinceByCountryResponse>>
{
    public async Task<List<GetProvinceByCountryResponse>> Handle(
        GetProvinceByCountryQuery request,
        CancellationToken cancellationToken)
    {
        var provinces = await provinceRepository.GetByCountryIdAsync(request.CountryId, cancellationToken);
        return [.. provinces.Select(p => new GetProvinceByCountryResponse(
            ProvinceId: p.Id,
            Code: p.Code,
            Name: p.Name,
            CountryId: p.CountryId
        ))];
    }
}

using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Municipality.GetMunicipalityByProvince;

public class GetMunicipalityByProvinceQueryHandler(IMunicipalityRepository municipalityRepository)
    : IRequestHandler<GetMunicipalityByProvinceQuery, List<GetMunicipalityByProvinceResponse>>
{
    public async Task<List<GetMunicipalityByProvinceResponse>> Handle(
        GetMunicipalityByProvinceQuery request,
        CancellationToken cancellationToken)
    {
        var municipalities = await municipalityRepository.GetByProvinceIdAsync(request.ProvinceId, cancellationToken);
        return [.. municipalities.Select(m => new GetMunicipalityByProvinceResponse(
            MunicipalityId: m.Id,
            Code: m.Code,
            Name: m.Name,
            ProvinceId: m.ProvinceId
        ))];
    }
}

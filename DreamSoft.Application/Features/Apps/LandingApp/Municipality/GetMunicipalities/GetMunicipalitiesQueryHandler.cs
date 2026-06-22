using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Municipality.GetMunicipalities;

public class GetMunicipalitiesQueryHandler(IMunicipalityRepository municipalityRepository)
    : IRequestHandler<GetMunicipalitiesQuery, List<GetMunicipalityResponse>>
{
    public async Task<List<GetMunicipalityResponse>> Handle(
        GetMunicipalitiesQuery request,
        CancellationToken cancellationToken)
    {
        var municipalities = await municipalityRepository.FindAsync(m => m.IsActive, cancellationToken);
        return [.. municipalities.Select(m => new GetMunicipalityResponse(
            MunicipalityId: m.Id,
            Code: m.Code,
            Name: m.Name,
            ProvinceId: m.ProvinceId
        ))];
    }
}

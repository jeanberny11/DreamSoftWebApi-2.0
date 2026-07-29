using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Municipality.GetMunicipalityByProvince;

public record GetMunicipalityByProvinceQuery(int ProvinceId) : IRequest<List<GetMunicipalityByProvinceResponse>>;

public record GetMunicipalityByProvinceResponse(int MunicipalityId, string Code, string Name, int ProvinceId);

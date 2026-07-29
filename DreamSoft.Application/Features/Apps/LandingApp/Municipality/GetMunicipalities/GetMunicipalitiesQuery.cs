using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Municipality.GetMunicipalities;

public record GetMunicipalitiesQuery : IRequest<List<GetMunicipalityResponse>>;

public record GetMunicipalityResponse(int MunicipalityId, string Code, string Name, int ProvinceId);

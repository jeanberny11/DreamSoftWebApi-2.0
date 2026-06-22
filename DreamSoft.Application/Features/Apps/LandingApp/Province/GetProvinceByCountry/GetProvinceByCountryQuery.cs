using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Province.GetProvinceByCountry;

public record GetProvinceByCountryQuery(int CountryId) : IRequest<List<GetProvinceByCountryResponse>>;

public record GetProvinceByCountryResponse(int ProvinceId, string Code, string Name, int CountryId);

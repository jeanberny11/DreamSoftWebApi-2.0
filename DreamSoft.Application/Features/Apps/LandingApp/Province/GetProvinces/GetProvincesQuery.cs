using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Province.GetProvinces;

public record GetProvincesQuery : IRequest<List<GetProvinceResponse>>;

public record GetProvinceResponse(int ProvinceId, string Code, string Name, int CountryId);

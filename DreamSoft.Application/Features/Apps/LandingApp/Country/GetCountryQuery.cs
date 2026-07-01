using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Country;

public record GetCountryQuery(string? Language = null) : IRequest<List<GetCountryResponse>>;

public record GetCountryResponse(
    int CountryId,
    string Code,
    string Name,
    string IsoCode,
    string PhoneCode
);

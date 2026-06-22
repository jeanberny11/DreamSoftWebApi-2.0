using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Country;

public class GetCountryQueryHandler(ICountryRepository countryRepository)
    : IRequestHandler<GetCountryQuery, List<GetCountryResponse>>
{
    public async Task<List<GetCountryResponse>> Handle(
        GetCountryQuery request,
        CancellationToken cancellationToken)
    {
        var language = request.Language ?? "es";
        var countries = await countryRepository.FindAsync(c => c.IsActive, cancellationToken);
        return [.. countries.Select(c => new GetCountryResponse(
            CountryId: c.Id,
            Code: c.Code,
            Name: c.GetTranslatedName(language),
            IsoCode: c.IsoCode,
            PhoneCode: c.PhoneCode
        ))];
    }
}

using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Language;

public class GetLanguageQueryHandler(ILanguageRepository languageRepository)
    : IRequestHandler<GetLanguageQuery, List<GetLanguageResponse>>
{
    public async Task<List<GetLanguageResponse>> Handle(
        GetLanguageQuery request,
        CancellationToken cancellationToken)
    {
        var language = request.Language ?? "es";

        var languages = await languageRepository.GetAllActiveAsync(cancellationToken);

        return [.. languages.Select(l => new GetLanguageResponse(
            LanguageId: l.Id,
            Code: l.Code,
            Name: l.GetTranslatedName(language),
            IsDefault: l.IsDefault
        ))];
    }
}

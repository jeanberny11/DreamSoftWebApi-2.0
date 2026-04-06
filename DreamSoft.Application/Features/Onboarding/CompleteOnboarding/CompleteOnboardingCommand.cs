using MediatR;

namespace DreamSoft.Application.Features.Onboarding.CompleteOnboarding;

public record CompleteOnboardingCommand(
    string? Phone,
    string? Website,
    string? AddressLine1,
    string? AddressLine2,
    int? CountryId,
    int? ProvinceId,
    int? MunicipalityId,
    string? PostalCode,
    int LanguageId
) : IRequest<CompleteOnboardingResponse>;

public record CompleteOnboardingResponse(string RedirectUrl);

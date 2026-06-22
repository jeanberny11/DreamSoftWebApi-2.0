using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Language;
  public record GetLanguageQuery(string? Language = null) : IRequest<List<GetLanguageResponse>>;

  public record GetLanguageResponse(
    int LanguageId,
    string Code,
    string Name,
    bool IsDefault = false
  );
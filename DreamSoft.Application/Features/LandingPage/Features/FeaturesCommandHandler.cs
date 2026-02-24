using DreamSoft.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DreamSoft.Application.Features.LandingPage.Features;

public class FeaturesCommandHandler(IApplicationDbContext context, ILogger<FeaturesCommandHandler> logger) : IRequestHandler<FeatureRequest, List<FeatureResponse>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<FeaturesCommandHandler> _logger = logger;

    public async Task<List<FeatureResponse>> Handle(FeatureRequest request, CancellationToken cancellationToken)
    {
        var language = request.Language;
        _logger.LogInformation(
            "Features requested with language: {Language}",
            language);
        var modules = await _context.Modules.Where(m => m.IsActive).ToListAsync(cancellationToken);
        return [.. modules.Select(m => new FeatureResponse {
            Name = m.Translations.GetNameOrFallback(language,m.Name),
            Description = m.Translations.GetDescriptionOrFallback(language,m.Description), Icon = m.Icon, SortOrder = m.SortOrder })];
    }
}
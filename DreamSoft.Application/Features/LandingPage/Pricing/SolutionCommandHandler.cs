using DreamSoft.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DreamSoft.Application.Features.LandingPage.Pricing;

public class SolutionCommandHandler(IApplicationDbContext context, ILogger<SolutionCommandHandler> logger) : IRequestHandler<SolutionRequest, List<SolutionDTO>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly ILogger<SolutionCommandHandler> _logger = logger;
    public async Task<List<SolutionDTO>> Handle(SolutionRequest request, CancellationToken cancellationToken)
    {
        var language = request.Language;
        _logger.LogInformation(
            "Features requested with language: {Language}",
            language);
        var solutions = await _context.Solutions.Where(s => s.IsActive).ToListAsync(cancellationToken);
        var resulttasks = solutions.Select( async s => new SolutionDTO {
            Code = s.Code,
            Name = s.Translations.GetNameOrFallback(language,s.Name),
            Description = s.Translations.GetDescriptionOrFallback(language,s.Description),
            Icon = s.Icon,
            SortOrder = s.SortOrder,
            SolutionOptions =   await GetSolutionOptions(s.Id, language, cancellationToken)
        });
        return [.. await Task.WhenAll(resulttasks)];
    }

    private async Task<List<SolutionOptionDTO>> GetSolutionOptions(int solutionId, string language, CancellationToken cancellationToken)
    {
        var menuOtions = await _context.SolutionMenuOptions.Where(sm => sm.SolutionId == solutionId).ToListAsync(cancellationToken);
        return [.. menuOtions.Select(mo => new SolutionOptionDTO {
            Code = mo.MenuOption.Code,
            Name = mo.MenuOption.Translations.GetNameOrFallback(language,mo.MenuOption.Name),
            Description = mo.MenuOption.Translations.GetDescriptionOrFallback(language,mo.MenuOption.Description),
            Icon = mo.MenuOption.Icon,
            SortOrder = mo.MenuOption.SortOrder,
            ModuleName = mo.MenuOption.Module.Name
        })];
    }
}
using DreamSoft.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Application.Features.LandingPage.Pricing;

public class SolutionCommandHandler(IApplicationDbContext context) : IRequestHandler<SolutionRequest, List<SolutionDTO>>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<List<SolutionDTO>> Handle(SolutionRequest request, CancellationToken cancellationToken)
    {
        var language = request.Language;
        var solutions = await _context.Solutions.Where(s => s.IsActive).ToListAsync(cancellationToken);
        var resultTasks = solutions.Select(async s => new SolutionDTO {
            Code = s.Code,
            Name = s.Translations.GetNameOrFallback(language, s.Name),
            Description = s.Translations.GetDescriptionOrFallback(language, s.Description),
            Icon = s.Icon,
            SortOrder = s.SortOrder,
            SolutionOptions = await GetSolutionOptions(s.Id, language, cancellationToken)
        });
        return [.. await Task.WhenAll(resultTasks)];
    }

    private async Task<List<SolutionOptionDTO>> GetSolutionOptions(int solutionId, string language, CancellationToken cancellationToken)
    {
        var planMenuOptions = await _context.PlanMenuOptions
            .Where(pmo => pmo.Plan.SolutionId == solutionId)
            .Include(pmo => pmo.MenuOption).ThenInclude(mo => mo.Module)
            .ToListAsync(cancellationToken);
        var menuOptions = planMenuOptions
            .Select(pmo => pmo.MenuOption)
            .DistinctBy(mo => mo.Id)
            .ToList();
        return [.. menuOptions.Select(mo => new SolutionOptionDTO {
            Code = mo.Code,
            Name = mo.Translations.GetNameOrFallback(language, mo.Name),
            Description = mo.Translations.GetDescriptionOrFallback(language, mo.Description),
            Icon = mo.Icon,
            SortOrder = mo.SortOrder,
            ModuleName = mo.Module.Name
        })];
    }
}

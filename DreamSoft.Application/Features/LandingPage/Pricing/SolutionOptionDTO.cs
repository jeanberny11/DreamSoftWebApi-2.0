namespace DreamSoft.Application.Features.LandingPage.Pricing;

public class SolutionOptionDTO
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Icon { get; set; } = null!;
    public int SortOrder { get; set; }
    public string ModuleName { get; set; } = null!;
}
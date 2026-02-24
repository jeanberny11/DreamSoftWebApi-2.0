namespace DreamSoft.Application.Features.LandingPage.Features;

public class FeatureResponse
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Icon { get; set; } = null!;
    public int SortOrder { get; set; }
}
namespace DreamSoft.Domain.Entities;

public class SolutionMenuOption
{
    public int SolutionId { get; set; }
    public int MenuOptionId { get; set; }

    // Navigation properties
    public Solution Solution { get; set; } = null!;
    public MenuOption MenuOption { get; set; } = null!;

    private SolutionMenuOption() { }

    public static SolutionMenuOption Create(int solutionId, int menuOptionId)
    {
        if (solutionId <= 0)
            throw new ArgumentException("Solution ID must be greater than zero", nameof(solutionId));

        if (menuOptionId <= 0)
            throw new ArgumentException("Menu option ID must be greater than zero", nameof(menuOptionId));

        return new SolutionMenuOption
        {
            SolutionId = solutionId,
            MenuOptionId = menuOptionId
        };
    }
}

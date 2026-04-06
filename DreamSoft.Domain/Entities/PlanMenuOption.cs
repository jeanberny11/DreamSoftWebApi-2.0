namespace DreamSoft.Domain.Entities;

public class PlanMenuOption
{
    public int PlanId { get; set; }
    public int MenuOptionId { get; set; }

    // Navigation properties
    public SubscriptionPlan Plan { get; set; } = null!;
    public MenuOption MenuOption { get; set; } = null!;

    private PlanMenuOption() { }

    public static PlanMenuOption Create(int planId, int menuOptionId)
    {
        if (planId <= 0)
            throw new ArgumentException("Plan ID must be greater than zero", nameof(planId));

        if (menuOptionId <= 0)
            throw new ArgumentException("Menu option ID must be greater than zero", nameof(menuOptionId));

        return new PlanMenuOption { PlanId = planId, MenuOptionId = menuOptionId };
    }
}

using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class RoleOptionAction : BaseEntity
{
    public int RoleId { get; set; }
    public int MenuOptionId { get; set; }
    public int ActionId { get; set; }

    // Navigation properties
    public Role Role { get; set; } = null!;
    public MenuOption MenuOption { get; set; } = null!;
    public OptionAction Action { get; set; } = null!;

    private RoleOptionAction() { }

    public static RoleOptionAction Create(
        int roleId,
        int menuOptionId,
        int actionId)
    {
        if (roleId <= 0)
            throw new ArgumentException("Role ID must be greater than zero", nameof(roleId));

        if (menuOptionId <= 0)
            throw new ArgumentException("Menu option ID must be greater than zero", nameof(menuOptionId));

        if (actionId <= 0)
            throw new ArgumentException("Action ID must be greater than zero", nameof(actionId));

        var roleOptionAction = new RoleOptionAction
        {
            RoleId = roleId,
            MenuOptionId = menuOptionId,
            ActionId = actionId
        };
        return roleOptionAction;
    }
}

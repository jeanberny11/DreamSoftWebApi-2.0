using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class RoleOptionActionTemplate : BaseEntity
{
    public int RoleTemplateId { get; set; }
    public int MenuOptionId { get; set; }
    public int ActionId { get; set; }

    // Navigation properties
    public RoleTemplate RoleTemplate { get; set; } = null!;
    public MenuOption MenuOption { get; set; } = null!;
    public OptionAction Action { get; set; } = null!;

    private RoleOptionActionTemplate() { }

    public static RoleOptionActionTemplate Create(int roleTemplateId, int menuOptionId, int actionId)
    {
        if (roleTemplateId <= 0)
            throw new ArgumentException("Role template ID must be greater than zero", nameof(roleTemplateId));

        if (menuOptionId <= 0)
            throw new ArgumentException("Menu option ID must be greater than zero", nameof(menuOptionId));

        if (actionId <= 0)
            throw new ArgumentException("Action ID must be greater than zero", nameof(actionId));

        return new RoleOptionActionTemplate
        {
            RoleTemplateId = roleTemplateId,
            MenuOptionId = menuOptionId,
            ActionId = actionId
        };
    }
}

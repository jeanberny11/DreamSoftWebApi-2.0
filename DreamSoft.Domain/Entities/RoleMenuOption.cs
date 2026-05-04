using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class RoleMenuOption : BaseEntity
{
    public int RoleId { get; set; }
    public int MenuOptionId { get; set; }

    // Navigation properties
    public Role Role { get; set; } = null!;
    public MenuOption MenuOption { get; set; } = null!;

    private RoleMenuOption() { }

    public static RoleMenuOption Create(int roleId, int menuOptionId)
    {
        if (roleId <= 0)
            throw new ArgumentException("Role ID must be greater than zero", nameof(roleId));

        if (menuOptionId <= 0)
            throw new ArgumentException("Menu option ID must be greater than zero", nameof(menuOptionId));

        return new RoleMenuOption { RoleId = roleId, MenuOptionId = menuOptionId };
    }
}

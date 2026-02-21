using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class RoleMenuOptionTemplate : BaseEntity
{
    public int RoleId { get; set; }
    public int MenuOptionId { get; set; }

    // Navigation properties
    public RoleTemplate Role { get; set; } = null!;
    public MenuOption MenuOption { get; set; } = null!;

    private RoleMenuOptionTemplate() { }

    public static RoleMenuOptionTemplate Create(int roleId, int menuOptionId)
    {
        if (roleId <= 0)
            throw new ArgumentException("Role ID must be greater than zero", nameof(roleId));

        if (menuOptionId <= 0)
            throw new ArgumentException("Menu option ID must be greater than zero", nameof(menuOptionId));

        return new RoleMenuOptionTemplate
        {
            RoleId = roleId,
            MenuOptionId = menuOptionId
        };
    }
}

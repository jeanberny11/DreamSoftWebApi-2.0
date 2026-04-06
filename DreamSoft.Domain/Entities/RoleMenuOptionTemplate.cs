using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class RoleMenuOptionTemplate : BaseEntity
{
    public int RoleTemplateId { get; set; }
    public int MenuOptionId { get; set; }

    // Navigation properties
    public RoleTemplate RoleTemplate { get; set; } = null!;
    public MenuOption MenuOption { get; set; } = null!;

    private RoleMenuOptionTemplate() { }

    public static RoleMenuOptionTemplate Create(int roleTemplateId, int menuOptionId)
    {
        if (roleTemplateId <= 0)
            throw new ArgumentException("Role template ID must be greater than zero", nameof(roleTemplateId));

        if (menuOptionId <= 0)
            throw new ArgumentException("Menu option ID must be greater than zero", nameof(menuOptionId));

        return new RoleMenuOptionTemplate
        {
            RoleTemplateId = roleTemplateId,
            MenuOptionId = menuOptionId
        };
    }
}

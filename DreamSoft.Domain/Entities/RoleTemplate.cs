using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class RoleTemplate : LookupEntity
{
    public string Code { get; set; } = null!;
    public string Description { get; set; } = "";

    // Navigation properties
    public ICollection<Role> Roles { get; private set; } = [];
    public ICollection<RoleMenuOptionTemplate> RoleMenuOptionTemplates { get; private set; } = [];
    public ICollection<RoleOptionActionTemplate> RoleOptionActionTemplates { get; private set; } = [];

    private RoleTemplate() { }

    public static RoleTemplate Create(string code, string name, string description = "")
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        var roleTemplate = new RoleTemplate
        {
            Code = code.ToUpper().Trim(),
            Name = name.Trim(),
            Description = description.Trim()
        };

        roleTemplate.InitializeAudit();
        return roleTemplate;
    }

    public void UpdateDescription(string description)
    {
        Description = description.Trim();
        MarkAsUpdated();
    }
}

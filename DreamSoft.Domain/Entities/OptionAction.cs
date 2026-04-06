using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class OptionAction : LookupEntity
{
    public string Code { get; set; } = null!;
    public string Description { get; set; } = "";

    // Navigation properties
    public ICollection<RoleOptionAction> RoleOptionActions { get; private set; } = [];
    public ICollection<RoleOptionActionTemplate> RoleOptionActionTemplates { get; private set; } = [];

    private OptionAction() { }

    public static OptionAction Create(string code, string name, TranslatedString translations, string description = "")
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations, nameof(translations));

        var optionAction = new OptionAction
        {
            Code = code.ToUpper().Trim(),
            Name = name.Trim(),
            Description = description.Trim(),
            Translations = translations
        };

        optionAction.InitializeAudit();
        return optionAction;
    }

    public void UpdateDetails(string name, string description, TranslatedString translations)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations, nameof(translations));

        Name = name.Trim();
        Description = description.Trim();
        UpdateTranslations(translations);
    }
}

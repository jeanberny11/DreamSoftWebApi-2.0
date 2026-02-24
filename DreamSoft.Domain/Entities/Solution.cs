using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class Solution : LookupEntity
{
    public string Code { get; set; } = null!;
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "";
    public int SortOrder { get; set; }

    // Navigation properties
    public ICollection<SubscriptionPlan> SubscriptionPlans { get; private set; } = [];
    public ICollection<SolutionMenuOption> SolutionMenuOptions { get; private set; } = [];

    private Solution() { }

    public static Solution Create(
        string code,
        string name,
        TranslatedString translations,
        string description = "",
        string icon = "",
        int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations, nameof(translations));

        var solution = new Solution
        {
            Code = code.ToUpper().Trim(),
            Name = name.Trim(),
            Description = description.Trim(),
            Translations = translations,
            Icon = icon.Trim(),
            SortOrder = sortOrder
        };

        solution.InitializeAudit();
        return solution;
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

    public void UpdateIcon(string icon)
    {
        Icon = icon.Trim();
        MarkAsUpdated();
    }

    public void UpdateSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
        MarkAsUpdated();
    }
}

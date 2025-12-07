using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class SubscriptionTier : LookupEntity
{
    public string Code { get; protected set; } = null!;
    public int Level { get; protected set; }
    public string? Description { get; protected set; }

    // Navigation properties
    public ICollection<SubscriptionPlan> SubscriptionPlans { get; private set; } = [];

    private SubscriptionTier() { }

    public static SubscriptionTier Create(string code, string name, int level, TranslatedString? translations = null, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        if (level < 0)
            throw new ArgumentException("Level must be non-negative", nameof(level));

        var tier = new SubscriptionTier
        {
            Code = code.ToLower().Trim(),
            Name = name.Trim(),
            Level = level,
            Description = description?.Trim(),
            Translations = translations
        };

        tier.InitializeAudit();
        return tier;
    }
}

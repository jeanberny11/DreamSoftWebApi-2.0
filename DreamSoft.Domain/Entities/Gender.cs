using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class Gender : LookupEntity
{
    public string Code { get; set; } = null!;

    // Navigation properties
    public ICollection<User> Users { get; private set; } = [];

    private Gender() { }

    public static Gender Create(string code, string name, TranslatedString translations)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations, nameof(translations));

        var gender = new Gender
        {
            Code = code.ToUpper().Trim(),
            Name = name.Trim(),
            Translations = translations
        };

        gender.InitializeAudit();
        return gender;
    }
}

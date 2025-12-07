using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class ProductType : LookupEntity
{
    // Navigation properties
    public ICollection<Product> Products { get; private set; } = [];

    private ProductType() { }

    public static ProductType Create(string name, TranslatedString translations)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations);

        var productType = new ProductType
        {
            Name = name.Trim(),
            Translations = translations
        };

        productType.InitializeAudit();
        return productType;
    }
}

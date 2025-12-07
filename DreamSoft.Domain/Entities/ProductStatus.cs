using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class ProductStatus : LookupEntity
{
    // Navigation properties
    public ICollection<Product> Products { get; private set; } = [];

    private ProductStatus() { }

    public static ProductStatus Create(string name, TranslatedString translations)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations);

        var productStatus = new ProductStatus
        {
            Name = name.Trim(),
            Translations = translations
        };

        productStatus.InitializeAudit();
        return productStatus;
    }
}

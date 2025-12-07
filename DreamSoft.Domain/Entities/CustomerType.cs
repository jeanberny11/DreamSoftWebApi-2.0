using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class CustomerType : LookupEntity
{
    // Navigation properties
    public ICollection<Customer> Customers { get; private set; } = [];

    private CustomerType() { }

    public static CustomerType Create(string name, TranslatedString translations)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations);

        var customerType = new CustomerType
        {
            Name = name.Trim(),
            Translations = translations
        };

        customerType.InitializeAudit();
        return customerType;
    }
}

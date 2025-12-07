using DreamSoft.Domain.Common;
using DreamSoft.Domain.ValueObjects;

namespace DreamSoft.Domain.Entities;

public class CustomerStatus : LookupEntity
{
    // Navigation properties
    public ICollection<Customer> Customers { get; private set; } = [];

    private CustomerStatus() { }

    public static CustomerStatus Create(string name, TranslatedString translations)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        ArgumentNullException.ThrowIfNull(translations);

        var customerStatus = new CustomerStatus
        {
            Name = name.Trim(),
            Translations = translations
        };

        customerStatus.InitializeAudit();
        return customerStatus;
    }
}

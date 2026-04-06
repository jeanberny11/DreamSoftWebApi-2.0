using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class SubscriptionPayment : AuditableEntity
{
    public int SubscriptionInvoiceId { get; private set; }
    public int TenantId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;
    public string Status { get; private set; } = null!;
    public DateTime PaymentDate { get; private set; }
    public string? StripePaymentId { get; private set; }
    public string? FailureReason { get; private set; }

    // Navigation properties
    public SubscriptionInvoice Invoice { get; private set; } = null!;
    public Tenant Tenant { get; private set; } = null!;

    private SubscriptionPayment() { }

    public static SubscriptionPayment Create(
        int subscriptionInvoiceId,
        int tenantId,
        decimal amount,
        string currency,
        string status,
        DateTime paymentDate,
        string? stripePaymentId = null,
        string? failureReason = null)
    {
        if (subscriptionInvoiceId <= 0)
            throw new ArgumentException("Invoice ID must be greater than zero", nameof(subscriptionInvoiceId));

        if (tenantId <= 0)
            throw new ArgumentException("Tenant ID must be greater than zero", nameof(tenantId));

        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required", nameof(currency));

        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status is required", nameof(status));

        var payment = new SubscriptionPayment
        {
            SubscriptionInvoiceId = subscriptionInvoiceId,
            TenantId = tenantId,
            Amount = amount,
            Currency = currency.ToUpper().Trim(),
            Status = status.ToLower().Trim(),
            PaymentDate = paymentDate,
            StripePaymentId = stripePaymentId?.Trim(),
            FailureReason = failureReason?.Trim()
        };

        payment.InitializeAudit();
        return payment;
    }
}

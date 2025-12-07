using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class Payment : AuditableEntity
{
    public int TenantId { get; protected set; }
    public int? SubscriptionId { get; protected set; }
    public decimal Amount { get; protected set; }
    public string Currency { get; protected set; } = "USD";
    public string Status { get; protected set; } = null!;
    public string? StripePaymentIntentId { get; protected set; }
    public DateTime? PaidAt { get; protected set; }

    // Navigation properties
    public Tenant Tenant { get; private set; } = null!;
    public TenantSubscription? Subscription { get; private set; }

    private Payment() { }

    public static Payment Create(int tenantId, decimal amount, string status, string currency = "USD", int? subscriptionId = null, string? stripePaymentIntentId = null, DateTime? paidAt = null)
    {
        if (tenantId <= 0)
            throw new ArgumentException("Tenant ID is required", nameof(tenantId));

        if (amount < 0)
            throw new ArgumentException("Amount must be non-negative", nameof(amount));

        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status is required", nameof(status));

        var payment = new Payment
        {
            TenantId = tenantId,
            SubscriptionId = subscriptionId,
            Amount = amount,
            Currency = currency.ToUpper().Trim(),
            Status = status.ToLower().Trim(),
            StripePaymentIntentId = stripePaymentIntentId?.Trim(),
            PaidAt = paidAt
        };

        payment.InitializeAudit();
        return payment;
    }
}

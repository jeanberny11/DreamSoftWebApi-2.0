using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class SubscriptionInvoice : AuditableEntity
{
    public int TenantId { get; private set; }
    public int TenantSubscriptionId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;
    public string Status { get; private set; } = null!;
    public DateTime DueDate { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public string? StripeInvoiceId { get; private set; }
    public string? StripePaymentIntentId { get; private set; }
    public string? Notes { get; private set; }
    public string? HostedInvoiceUrl { get; private set; }
    public string? InvoicePdfUrl { get; private set; }
    public string? InvoiceNumber { get; private set; }
    public string? BillingReason { get; private set; }

    // Navigation properties
    public Tenant Tenant { get; private set; } = null!;
    public TenantSubscription TenantSubscription { get; private set; } = null!;
    public ICollection<SubscriptionPayment> Payments { get; private set; } = [];

    private SubscriptionInvoice() { }

    public static SubscriptionInvoice Create(
        int tenantId,
        int tenantSubscriptionId,
        decimal amount,
        string currency,
        DateTime dueDate,
        string? stripeInvoiceId = null)
    {
        if (tenantId <= 0)
            throw new ArgumentException("Tenant ID must be greater than zero", nameof(tenantId));

        if (tenantSubscriptionId <= 0)
            throw new ArgumentException("Tenant subscription ID must be greater than zero", nameof(tenantSubscriptionId));

        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required", nameof(currency));

        var invoice = new SubscriptionInvoice
        {
            TenantId = tenantId,
            TenantSubscriptionId = tenantSubscriptionId,
            Amount = amount,
            Currency = currency.ToUpper().Trim(),
            Status = "unpaid",
            DueDate = dueDate,
            StripeInvoiceId = stripeInvoiceId?.Trim()
        };

        invoice.InitializeAudit();
        return invoice;
    }

    public void MarkAsPaid(DateTime paidAt, string? stripePaymentIntentId = null)
    {
        Status = "paid";
        PaidAt = paidAt;
        StripePaymentIntentId = stripePaymentIntentId?.Trim();
        MarkAsUpdated();
    }

    public void MarkAsFailed() { Status = "failed"; MarkAsUpdated(); }
    public void MarkAsRefunded() { Status = "refunded"; MarkAsUpdated(); }
    public void UpdateNotes(string? notes) { Notes = notes?.Trim(); MarkAsUpdated(); }

    public void UpdateStripeDetails(
        string? hostedInvoiceUrl,
        string? invoicePdfUrl,
        string? invoiceNumber,
        string? billingReason)
    {
        HostedInvoiceUrl = hostedInvoiceUrl?.Trim();
        InvoicePdfUrl    = invoicePdfUrl?.Trim();
        InvoiceNumber    = invoiceNumber?.Trim();
        BillingReason    = billingReason?.Trim();
        MarkAsUpdated();
    }
}

namespace DreamSoft.Domain.Constants;

/// <summary>
/// Type-safe constants for the subscription_statuses.code column.
/// </summary>
public static class SubscriptionStatusCodes
{
    public const string Trial = "TRIAL";
    public const string Active = "ACTIVE";
    public const string PastDue = "PAST_DUE";
    public const string Suspended = "SUSPENDED";
    public const string Cancelled = "CANCELLED";
    public const string Expired = "EXPIRED";
    public const string ProcessingPayment = "PROCESSING_PAYMENT";

    /// <summary>
    /// First-time checkout was abandoned or card declined before any payment
    /// was ever collected. The subscription record exists but is not active.
    /// The tenant must retry via the retry-payment endpoint.
    /// </summary>
    public const string PaymentFailed = "PAYMENT_FAILED";
}

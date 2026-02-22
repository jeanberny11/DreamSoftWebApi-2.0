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
}

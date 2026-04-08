namespace DreamSoft.Application.Common.Exceptions;

/// <summary>
/// Programmatic error codes used with ForbiddenException.
/// These are passed through to the client so it can react to each specific blocked state.
/// Defined in the Application layer to keep handlers decoupled from the API layer.
/// </summary>
public static class ForbiddenErrorCodes
{
    public const string AccountLocked                  = "ACCOUNT_LOCKED";
    public const string EmailNotVerified               = "EMAIL_NOT_VERIFIED";
    public const string TenantPendingEmailVerification = "TENANT_PENDING_EMAIL_VERIFICATION";
    public const string TenantPendingSubscription      = "TENANT_PENDING_SUBSCRIPTION";
    public const string TenantSuspended                = "TENANT_SUSPENDED";
    public const string TenantCancelled                = "TENANT_CANCELLED";

    public const string SubscriptionNotFound      = "SUBSCRIPTION_NOT_FOUND";
    public const string SubscriptionPastDue       = "SUBSCRIPTION_PAST_DUE";
    public const string SubscriptionSuspended     = "SUBSCRIPTION_SUSPENDED";
    public const string SubscriptionCancelled     = "SUBSCRIPTION_CANCELLED";
    public const string SubscriptionExpired       = "SUBSCRIPTION_EXPIRED";
    public const string SubscriptionPaymentFailed = "SUBSCRIPTION_PAYMENT_FAILED";

    public const string RoleNotAuthorized = "ROLE_NOT_AUTHORIZED";
}

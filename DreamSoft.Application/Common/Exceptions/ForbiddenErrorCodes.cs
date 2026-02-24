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
}

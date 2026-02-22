namespace DreamSoft.Domain.Constants;

/// <summary>
/// Type-safe constants for the tenant_statuses.code column.
/// All values match the seed data inserted in the migration.
/// </summary>
public static class TenantStatusCodes
{
    public const string PendingEmailVerification = "PENDING_EMAIL_VERIFICATION";
    public const string PendingSubscription = "PENDING_SUBSCRIPTION";
    public const string Active = "ACTIVE";
    public const string Suspended = "SUSPENDED";
    public const string Cancelled = "CANCELLED";
}

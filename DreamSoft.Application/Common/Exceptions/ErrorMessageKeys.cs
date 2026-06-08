namespace DreamSoft.Application.Common.Exceptions;

/// <summary>
/// Resource keys for localized error messages.
/// Each constant maps to an entry in ErrorMessages.resx (and its culture variants).
/// Use these when throwing application exceptions to avoid typos and broken lookups.
/// </summary>
public static class ErrorMessageKeys
{
    // ── Generic ─────────────────────────────────────────────────────────────
    public const string ValidationError     = "ValidationError";
    public const string InternalServerError = "InternalServerError";
    public const string NotFound            = "NotFound";
    public const string Conflict            = "Conflict";
    public const string Unauthorized        = "Unauthorized";
    public const string Forbidden           = "Forbidden";

    // ── Authentication ───────────────────────────────────────────────────────
    public const string InvalidCredentials  = "InvalidCredentials";
    public const string InvalidOtpCode      = "InvalidOtpCode";
    public const string InvalidRefreshToken = "InvalidRefreshToken";

    // ── Account / Tenant state ───────────────────────────────────────────────
    /// <summary>Requires {0} = remaining lockout minutes.</summary>
    public const string AccountLocked                  = "AccountLocked";
    public const string EmailNotVerified               = "EmailNotVerified";
    public const string EmailVerificationRequired      = "EmailVerificationRequired";
    public const string TenantPendingSubscription      = "TenantPendingSubscription";
    public const string AccountSuspended               = "AccountSuspended";
    public const string AccountCancelled               = "AccountCancelled";
    public const string OnboardingRequired             = "OnboardingRequired";

    // ── Conflict / Already-done ──────────────────────────────────────────────
    public const string BillingCycleCodeAlreadyExists    = "BillingCycleCodeAlreadyExists";
    public const string CurrencyCodeAlreadyExists        = "CurrencyCodeAlreadyExists";
    public const string EmailAlreadyExists               = "EmailAlreadyExists";
    public const string GenderCodeAlreadyExists          = "GenderCodeAlreadyExists";
    public const string LanguageCodeAlreadyExists        = "LanguageCodeAlreadyExists";
    public const string MenuGroupCodeAlreadyExists       = "MenuGroupCodeAlreadyExists";
    public const string MenuOptionCodeAlreadyExists      = "MenuOptionCodeAlreadyExists";
    public const string ModuleCodeAlreadyExists          = "ModuleCodeAlreadyExists";
    public const string PlanLimitKeyAlreadyExists        = "PlanLimitKeyAlreadyExists";
    public const string PlanMenuOptionAlreadyExists      = "PlanMenuOptionAlreadyExists";
    public const string PlanPriceAlreadyExists           = "PlanPriceAlreadyExists";
    public const string SolutionCodeAlreadyExists        = "SolutionCodeAlreadyExists";
    public const string SubscriptionPlanCodeAlreadyExists = "SubscriptionPlanCodeAlreadyExists";
    public const string TenantAlreadyExists     = "TenantAlreadyExists";
    public const string EmailAlreadyVerified    = "EmailAlreadyVerified";
    public const string OnboardingAlreadyComplete = "OnboardingAlreadyComplete";

    // ── Not found (specific) ─────────────────────────────────────────────────
    /// <summary>Requires {0} = user id or identifier.</summary>
    public const string UserNotFound   = "UserNotFound";
    /// <summary>Requires {0} = tenant id.</summary>
    public const string TenantNotFound = "TenantNotFound";
    /// <summary>Requires {0} = plan price id.</summary>
    public const string PlanPriceNotFound = "PlanPriceNotFound";
    /// <summary>Requires {0} = subscription plan id.</summary>
    public const string SubscriptionPlanNotFound = "SubscriptionPlanNotFound";
    /// <summary>Requires {0} = tenant id, {1} = solution id.</summary>
    public const string TenantSubscriptionNotFound = "TenantSubscriptionNotFound";

    // ── Rate limiting / Infrastructure ───────────────────────────────────────
    public const string RateLimitExceeded  = "RateLimitExceeded";
    public const string EmailSendFailed    = "EmailSendFailed";

    // ── Not found (subscription-specific) ───────────────────────────────────
    /// <summary>Requires {0} = subscription id.</summary>
    public const string SubscriptionNotFound  = "SubscriptionNotFound";
    public const string SubscriptionNotActive = "SubscriptionNotActive";

    // ── Subscription login-gate states ──────────────────────────────────────
    public const string SubscriptionPastDue       = "SubscriptionPastDue";
    public const string SubscriptionSuspended     = "SubscriptionSuspended";
    public const string SubscriptionCancelled     = "SubscriptionCancelled";
    public const string SubscriptionExpired       = "SubscriptionExpired";
    public const string SubscriptionPaymentFailed = "SubscriptionPaymentFailed";

    // ── Subscription state / business rules ─────────────────────────────────
    public const string SubscriptionNotOwnedByTenant      = "SubscriptionNotOwnedByTenant";
    public const string SubscriptionNotRetryable          = "SubscriptionNotRetryable";
    public const string SubscriptionNotCancellable        = "SubscriptionNotCancellable";
    public const string SubscriptionNotChangeable         = "SubscriptionNotChangeable";
    public const string SubscriptionPlanNotActive         = "SubscriptionPlanNotActive";
    public const string TenantAlreadySubscribedToSolution = "TenantAlreadySubscribedToSolution";
    public const string AlreadyOnThisPlan                 = "AlreadyOnThisPlan";
    public const string InvalidTenantStatus               = "InvalidTenantStatus";

    // ── Subscription / Pricing ───────────────────────────────────────────────
    public const string PlanNotBelongToSolution = "PlanNotBelongToSolution";
    public const string PlanPriceMismatch        = "PlanPriceMismatch";
    public const string PlanPriceNotActive       = "PlanPriceNotActive";

    // ── Stripe / Payment (user-safe messages for internal config errors) ──────
    public const string StripePriceIdNotConfigured = "StripePriceIdNotConfigured";
    public const string NoStripeSubscriptionFound  = "NoStripeSubscriptionFound";
    public const string NoStripeCustomerFound      = "NoStripeCustomerFound";

    // ── Menu / Roles ─────────────────────────────────────────────────────────
    public const string UserRoleNotAssigned = "UserRoleNotAssigned";
    /// <summary>Requires {0} = role id.</summary>
    public const string RoleNotFound = "RoleNotFound";
}

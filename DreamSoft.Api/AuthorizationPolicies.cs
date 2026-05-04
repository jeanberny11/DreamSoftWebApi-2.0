namespace DreamSoft.Api;

/// <summary>
/// Named JWT Bearer authentication schemes.
/// Used when registering AddJwtBearer() in Program.cs and when
/// specifying AuthenticationSchemes in [Authorize] attributes or policies.
/// </summary>
public static class AuthSchemes
{
    /// <summary>
    /// Scheme for tokens issued to Tenant account owners (TenantAuthController).
    /// Validated against Jwt:Tenant issuer, audience, and secret.
    /// </summary>
    public const string Tenant = "TenantScheme";

    /// <summary>
    /// Scheme for tokens issued to solution Users (AuthController).
    /// Validated against Jwt:User issuer, audience, and secret.
    /// </summary>
    public const string User = "UserScheme";

    /// <summary>Scheme for tokens issued to platform AdminUsers.</summary>
    public const string SuperAdmin = "SuperAdminScheme";
}

/// <summary>
/// Named Authorization Policy names.
/// Apply via [Authorize(Policy = AuthPolicies.TenantOnly)] on controllers or actions.
/// </summary>
public static class AuthPolicies
{
    /// <summary>
    /// Requires a valid Tenant token (token_type = "tenant").
    /// Use on: OnboardingController, SubscriptionController, and any
    /// endpoint that manages the tenant account itself.
    /// </summary>
    public const string TenantOnly = "TenantOnly";

    /// <summary>
    /// Requires a valid User token (token_type = "user").
    /// Use on: feature controllers, AuthController logout, and any
    /// endpoint that operates within a subscribed solution context.
    /// </summary>
    public const string UserOnly = "UserOnly";

    /// <summary>Requires a valid SuperAdmin token (token_type = "superadmin", role_code = "SUPER_ADMIN").</summary>
    public const string SuperAdminOnly = "SuperAdminOnly";
}

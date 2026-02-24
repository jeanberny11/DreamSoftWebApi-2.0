namespace DreamSoft.Api.Middleware;

/// <summary>
/// Resolves the tenant subdomain from the incoming <c>Host</c> header on every request
/// and stores it in <see cref="HttpContext.Items"/> under the key <c>"Subdomain"</c>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this runs before authentication:</b> public endpoints (landing page, pricing, etc.)
/// need tenant context before a JWT is ever issued, so we cannot rely solely on the
/// <c>tenant_id</c> JWT claim. The middleware runs unconditionally and populates
/// <c>HttpContext.Items["Subdomain"]</c> for any component that needs it downstream.
/// </para>
/// <para>
/// <b>Priority of tenant resolution:</b>
/// <list type="number">
///   <item>Authenticated requests: <c>tenant_id</c> JWT claim (resolved by <see cref="Application.Common.Interfaces.ICurrentUserService"/>).</item>
///   <item>Public requests: subdomain extracted here from the <c>Host</c> header.</item>
/// </list>
/// </para>
/// <para>
/// <b>Subdomain extraction rules:</b>
/// <list type="bullet">
///   <item><c>acme.dreamsoft.com</c> → <c>"acme"</c></item>
///   <item><c>dreamsoft.com</c> → <c>null</c> (root domain, no subdomain)</item>
///   <item><c>localhost</c> → <c>null</c> (development, no subdomain)</item>
/// </list>
/// </para>
/// </remarks>
public class TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
{
    /// <summary>Key used to store the resolved subdomain in <see cref="HttpContext.Items"/>.</summary>
    public const string SubdomainItemKey = "Subdomain";

    private readonly RequestDelegate _next = next;
    private readonly ILogger<TenantResolutionMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var subdomain = ResolveSubdomain(context.Request.Host.Host);

        if (subdomain is not null)
        {
            context.Items[SubdomainItemKey] = subdomain;
            _logger.LogDebug("Tenant subdomain resolved: {Subdomain}", subdomain);
        }

        await _next(context);
    }

    /// <summary>
    /// Extracts the subdomain segment from a hostname, or returns <c>null</c>
    /// when no subdomain is present.
    /// </summary>
    private static string? ResolveSubdomain(string? host)
    {
        if (string.IsNullOrWhiteSpace(host))
            return null;

        // Strip port if present (e.g. "acme.dreamsoft.com:5001" → "acme.dreamsoft.com")
        var hostWithoutPort = host.Split(':')[0];
        var parts = hostWithoutPort.Split('.');

        // Need at least 3 parts to have a subdomain: subdomain.domain.tld
        if (parts.Length >= 3)
            return parts[0].ToLowerInvariant();

        return null;
    }
}

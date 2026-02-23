using DreamSoft.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Application.Features.Registration.CheckSubdomainAvailability;

public class CheckSubdomainAvailabilityQueryHandler(IApplicationDbContext context)
    : IRequestHandler<CheckSubdomainAvailabilityQuery, SubdomainAvailabilityResponse>
{
    // Subdomains reserved for system use — cannot be registered by tenants
    private static readonly HashSet<string> Reserved = new(StringComparer.OrdinalIgnoreCase)
    {
        "www", "api", "admin", "app", "mail", "smtp", "ftp",
        "help", "support", "docs", "status", "cdn", "static",
        "dashboard", "portal", "login", "auth", "register",
        "billing", "account", "accounts", "dreamsoft"
    };

    public async Task<SubdomainAvailabilityResponse> Handle(
        CheckSubdomainAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        var normalised = request.Subdomain.Trim().ToLowerInvariant();

        // 1. Format validation (3–50 chars, alphanumeric + hyphens, no leading/trailing hyphen)
        if (normalised.Length < 3 || normalised.Length > 50)
            return new SubdomainAvailabilityResponse(normalised, false, "SubdomainLength");

        if (!System.Text.RegularExpressions.Regex.IsMatch(normalised, @"^[a-z0-9][a-z0-9\-]*[a-z0-9]$"))
            return new SubdomainAvailabilityResponse(normalised, false, "SubdomainFormat");

        // 2. Reserved name check
        if (Reserved.Contains(normalised))
            return new SubdomainAvailabilityResponse(normalised, false, "SubdomainReserved");

        // 3. Database uniqueness check (global — no tenant filter on Tenants)
        var taken = await context.Tenants
            .AnyAsync(t => t.Subdomain == normalised, cancellationToken);

        return taken
            ? new SubdomainAvailabilityResponse(normalised, false, "SubdomainTaken")
            : new SubdomainAvailabilityResponse(normalised, true);
    }
}

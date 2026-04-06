using System.Net;
using System.Text.Json;
using DreamSoft.Api.Contracts.Responses;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Api.Middleware;

public class TenantGatewayMiddleware(
    RequestDelegate next,
    ILogger<TenantGatewayMiddleware> logger)
{
    private static readonly string[] SkipPrefixes =
    [
        "/api/v1/registration",
        "/api/v1/onboarding",
        "/swagger",
        "/health",
    ];

    private readonly JsonSerializerOptions _json = new()
    { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Skip public/registration paths — no tenant gate applies
        if (SkipPrefixes.Any(p =>
                path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await next(context);
            return;
        }

        // Skip unauthenticated requests — auth middleware handles 401
        var currentUser = context.RequestServices
            .GetRequiredService<ICurrentUserService>();
        if (!currentUser.IsAuthenticated)
        {
            await next(context);
            return;
        }

        // Skip User tokens — the gateway only governs Tenant account status.
        // A User token already implies the tenant had an active subscription
        // at the time the user account was provisioned.
        if (currentUser.IsUserIdentity)
        {
            await next(context);
            return;
        }

        // No tenant claim — let the request through (auth middleware handles it)
        var tenantId = currentUser.TenantId;
        if (tenantId == null)
        {
            await next(context);
            return;
        }

        // Load tenant status code (lightweight projection — no full entity load)
        var dbContext = context.RequestServices
            .GetRequiredService<IApplicationDbContext>();

        var statusCode = await dbContext.Tenants
            .Where(t => t.Id == tenantId)
            .Select(t => t.Status.Code)
            .FirstOrDefaultAsync();

        var (block, errorCode, message) = statusCode switch
        {
            TenantStatusCodes.PendingEmailVerification =>
                (true, "EMAIL_VERIFICATION_REQUIRED",
                    "Please verify your email address to continue"),
            TenantStatusCodes.Suspended =>
                (true, "ACCOUNT_SUSPENDED",
                    "Your account has been suspended"),
            TenantStatusCodes.Cancelled =>
                (true, "ACCOUNT_CANCELLED",
                    "Your account has been cancelled"),
            _ => (false, string.Empty, string.Empty)
        };

        if (!block)
        {
            await next(context);
            return;
        }

        logger.LogWarning(
            "Gateway blocked request to {Path} for tenant {TenantId}: {ErrorCode}",
            path, tenantId, errorCode);

        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            StatusCode   = (int)HttpStatusCode.Forbidden,
            ErrorCode    = errorCode,
            ErrorType    = ErrorTypes.Forbidden,
            ErrorMessage = message,
            TraceId      = context.TraceIdentifier,
            Timestamp    = DateTime.UtcNow
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, _json));
    }
}

using System.Text;
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning.ApiExplorer;
using DreamSoft.Api;
using DreamSoft.Api.Middleware;
using DreamSoft.Application;
using DreamSoft.Infrastructure;
using DreamSoft.Infrastructure.Persistence;
using Microsoft.AspNetCore.Localization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Register Application layer services (MediatR, FluentValidation, AutoMapper, Behaviors)
builder.Services.AddApplication();

// Register Infrastructure layer services (DbContext, Repositories, Services)
builder.Services.AddInfrastructure(builder.Configuration);

// Configure Localization
builder.Services.AddLocalization();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en", "es" };

    options.SetDefaultCulture("es")
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);

    options.RequestCultureProviders.Insert(0, new AcceptLanguageHeaderRequestCultureProvider());
});

// ── JWT Authentication — two cryptographically isolated schemes ───────────────
//
//  TenantScheme  →  tokens issued by TenantAuthController
//                   signed with Jwt:Tenant:Secret
//                   issuer = "dreamsoft-tenant", audience = "dreamsoft-tenant-api"
//
//  UserScheme    →  tokens issued by AuthController
//                   signed with Jwt:User:Secret
//                   issuer = "dreamsoft-user", audience = "dreamsoft-user-api"
//
// Because the secrets, issuers, and audiences differ, a Tenant token cannot
// pass validation against UserScheme (and vice-versa) — even if one secret leaks.
// ─────────────────────────────────────────────────────────────────────────────

var tenantJwt = builder.Configuration.GetSection("Jwt:Tenant");
var userJwt = builder.Configuration.GetSection("Jwt:User");
var superAdminJwt = builder.Configuration.GetSection("Jwt:SuperAdmin");

var tenantSecret = tenantJwt["Secret"];
if (string.IsNullOrWhiteSpace(tenantSecret))
    throw new InvalidOperationException("Jwt:Tenant:Secret is not configured");

var userSecret = userJwt["Secret"];
if (string.IsNullOrWhiteSpace(userSecret))
    throw new InvalidOperationException("Jwt:User:Secret is not configured");

var superAdminSecret = superAdminJwt["Secret"];
if (string.IsNullOrWhiteSpace(superAdminSecret))
    throw new InvalidOperationException("Jwt:SuperAdmin:Secret is not configured");

builder.Services
    .AddAuthentication(options =>
    {
        // No default scheme — each policy specifies which scheme(s) to use.
        // This prevents accidental cross-scheme validation.
        options.DefaultAuthenticateScheme = null;
        options.DefaultChallengeScheme = null;
    })
    .AddJwtBearer(AuthSchemes.Tenant, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = tenantJwt["Issuer"],
            ValidAudience = tenantJwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tenantSecret)),
            ClockSkew = TimeSpan.Zero
        };
    })
    .AddJwtBearer(AuthSchemes.User, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = userJwt["Issuer"],
            ValidAudience = userJwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(userSecret)),
            ClockSkew = TimeSpan.Zero
        };
    })
    .AddJwtBearer(AuthSchemes.SuperAdmin, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = superAdminJwt["Issuer"],
            ValidAudience = superAdminJwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(superAdminSecret)),
            ClockSkew = TimeSpan.Zero
        };
    });

// ── Authorization Policies ────────────────────────────────────────────────────
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthPolicies.TenantOnly, policy =>
    {
        policy.AddAuthenticationSchemes(AuthSchemes.Tenant);
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("token_type", "tenant");
    })
    .AddPolicy(AuthPolicies.UserOnly, policy =>
    {
        policy.AddAuthenticationSchemes(AuthSchemes.User);
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("token_type", "user");
    })
    .AddPolicy(AuthPolicies.SuperAdminOnly, policy =>
    {
        policy.AddAuthenticationSchemes(AuthSchemes.SuperAdmin);
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("token_type", "superadmin");
        policy.RequireClaim("role_code", "SUPER_ADMIN");
    });

builder.Services.AddControllers();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, DreamSoft.Api.Swagger.ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

    options.AddSecurityDefinition("TenantBearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Tenant account token. Obtain from POST /api/v1/tenant-auth/login"
    });

    options.AddSecurityDefinition("UserBearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Solution user token. Obtain from POST /api/v1/auth/login"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "TenantBearer" }
            },
            Array.Empty<string>()
        }
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "UserBearer" }
            },
            Array.Empty<string>()
        }
    });

    options.AddSecurityDefinition("SuperAdminBearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Platform SuperAdmin token. Obtain from POST /api/v1/admin/auth/login"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "SuperAdminBearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── CORS ──────────────────────────────────────────────────────────────────────
// Development: explicit localhost origins from appsettings.Development.json
// Production:  subdomain wildcard for the configured production domain
builder.Services.AddCors(options =>
{
    options.AddPolicy("SubdomainPolicy", policy =>
    {
        // Read explicit allowed origins from config (used in development)
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>();

        // Extra explicit origins for production — a single comma-separated
        // value so it's easy to add/remove entries via one Railway env var
        // (Cors__AllowedOriginsExtra) without touching code or redeploying.
        // Useful for origins that don't match the wildcard below (e.g. a
        // Railway preview URL, or a custom domain before DNS cutover).
        var extraOrigins = builder.Configuration["Cors:AllowedOriginsExtra"]
            ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? [];

        // Production apex domain — tenants are served from subdomains of this
        // (e.g. acme.dreamsoft.com). Configurable via Cors:ProductionDomainSuffix
        // (or the Cors__ProductionDomainSuffix env var on Railway) so it can be
        // changed without a code change or redeploy.
        var productionDomainSuffix = builder.Configuration["Cors:ProductionDomainSuffix"] ?? "dreamsoft.com";

        policy
            .SetIsOriginAllowed(origin =>
            {
                // 1. Check explicit list from config (covers localhost:5173, etc.)
                if (allowedOrigins != null && allowedOrigins.Contains(origin))
                    return true;

                // 2. Check extra explicit production origins
                if (extraOrigins.Contains(origin))
                    return true;

                // 3. Allow any subdomain of the configured production domain
                var uri = new Uri(origin);
                return uri.Host.EndsWith("." + productionDomainSuffix) || uri.Host == productionDomainSuffix;
            })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

// ── Database: apply migrations + seed on startup ──────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db, config);
}

// ── HTTP Pipeline ─────────────────────────────────────────────────────────────
var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint(
            $"/swagger/{description.GroupName}/swagger.json",
            $"DreamSoft ERP API {description.GroupName.ToUpperInvariant()}");
    }
    options.RoutePrefix = string.Empty;
});

// HTTPS redirection — only in Development.
// Railway handles TLS termination at the proxy level; the container only
// receives plain HTTP internally, so redirecting would cause infinite loops.
if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseRequestLocalization();

// Exception handling — early in pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

// CORS — before Authentication/Authorization
app.UseCors("SubdomainPolicy");

// Tenant Resolution — populates HttpContext.Items["Subdomain"] from Host header
app.UseMiddleware<TenantResolutionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Blocks routes requiring active subscription/verified status
app.UseMiddleware<TenantGatewayMiddleware>();

app.MapControllers();

app.Run();

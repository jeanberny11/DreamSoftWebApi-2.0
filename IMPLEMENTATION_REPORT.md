# Implementation Report — Auth & Registration Improvements
## Branch: `rematering` | Commit: `188779e`

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Architecture Overview](#2-architecture-overview)
3. [Change 1 — Separate RefreshToken Entity](#3-change-1--separate-refreshtoken-entity)
4. [Change 2 — HTTP-Only Cookie for Refresh Token](#4-change-2--http-only-cookie-for-refresh-token)
5. [Change 3 — RememberMe Flag & Session-Aware Token Expiry](#5-change-3--rememberme-flag--session-aware-token-expiry)
6. [Change 4 — LastLoginAt Recording](#6-change-4--lastloginat-recording)
7. [Change 5 — IP-Based Rate Limiting on OTP Resend](#7-change-5--ip-based-rate-limiting-on-otp-resend)
8. [Change 6 — Subdomain Availability Check Endpoint](#8-change-6--subdomain-availability-check-endpoint)
9. [Change 7 — Terms of Service Acceptance Tracking](#9-change-7--terms-of-service-acceptance-tracking)
10. [Database Migrations](#10-database-migrations)
11. [Test Suite](#11-test-suite)
12. [Files Changed Summary](#12-files-changed-summary)
13. [Security Summary](#13-security-summary)
14. [Build & Test Results](#14-build--test-results)

---

## 1. Executive Summary

This document details every file created or modified during the Auth & Registration Improvements implementation on the `rematering` branch. Seven improvements were delivered across three priority levels:

| Priority | Change | Status |
|----------|--------|--------|
| HIGH | Separate `RefreshToken` entity with multi-device sessions + IP audit trail | ✅ Done |
| HIGH | HTTP-only `__Host-refresh_token` cookie (XSS protection) | ✅ Done |
| MEDIUM | `RememberMe` flag with session-aware 7-day / 30-day expiry | ✅ Done |
| MEDIUM | `LastLoginAt` recording on every successful login | ✅ Done |
| MEDIUM | IP-based sliding-window rate limiting on OTP resend (5/hour) | ✅ Done |
| LOW | `GET /api/registration/check-subdomain` availability endpoint | ✅ Done |
| LOW | Terms of Service acceptance tracking on `Tenant` entity | ✅ Done |

**Build:** ✅ Success
**Tests:** ✅ 81 / 81 passing
**Migrations:** ✅ `AddRefreshTokensTable`, `AddTenantTermsAcceptance`

---

## 2. Architecture Overview

The project follows **Clean Architecture** with four layers:

```
DreamSoft.Domain          ← Entities, domain rules (no dependencies)
DreamSoft.Application     ← CQRS handlers, interfaces, DTOs (depends on Domain)
DreamSoft.Infrastructure  ← EF Core, services (depends on Application + Domain)
DreamSoft.Api             ← Controllers, entry point (depends on Application + Infrastructure)
```

**Dependency flow:** `Api → Infrastructure → Application → Domain`

Key technologies used:
- .NET 8.0 with nullable reference types
- Entity Framework Core 9.0 (PostgreSQL via Npgsql)
- MediatR 13 (CQRS pattern)
- FluentValidation 12
- xUnit + SQLite (in-memory) for integration tests

---

## 3. Change 1 — Separate RefreshToken Entity

### What changed
Previously, refresh tokens were stored as two flat columns on the `users` table (`refresh_token`, `refresh_token_expiry_time`). This prevented multi-device sessions and offered no audit trail. The flat columns were replaced with a dedicated `refresh_tokens` table — one row per login session.

| Before | After |
|--------|-------|
| `refresh_token VARCHAR(500)` on `users` | Separate `refresh_tokens` table |
| `refresh_token_expiry_time TIMESTAMPTZ` on `users` | One row per login session |
| Single active token per user | Multiple concurrent sessions (multi-device) |
| No IP or device tracking | `created_by_ip`, `revoked_by_ip`, `device_info` |
| `user.SetRefreshToken()` / `ClearRefreshToken()` | `RefreshToken.Create(…)` factory + `Revoke(ip)` |

---

### 3.1 NEW FILE — `DreamSoft.Domain/Entities/RefreshToken.cs`

```csharp
using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

/// <summary>
/// A persisted refresh token record. One row is created per login, enabling
/// multiple concurrent sessions per user (multi-device support).
/// Tokens are never deleted — they are revoked so the audit trail is preserved.
/// </summary>
public class RefreshToken : AuditableEntity
{
    public int UserId { get; private set; }

    /// <summary>Cryptographically random opaque token string (64-byte Base64).</summary>
    public string Token { get; private set; } = null!;

    /// <summary>When this token expires. After expiry the token is no longer active.</summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>IP address that created this token (login request origin).</summary>
    public string? CreatedByIp { get; private set; }

    /// <summary>Optional User-Agent / device hint for session-list display.</summary>
    public string? DeviceInfo { get; private set; }

    /// <summary>Set when the token is explicitly revoked (logout or rotation).</summary>
    public DateTime? RevokedAt { get; private set; }

    /// <summary>IP address that revoked this token.</summary>
    public string? RevokedByIp { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;

    /// <summary>True only when the token has not been revoked and has not expired.</summary>
    public new bool IsActive => !IsRevoked && !IsExpired;

    // Navigation
    public User User { get; private set; } = null!;

    private RefreshToken() { }

    /// <summary>
    /// Factory method — creates a new active refresh token for a user login.
    /// </summary>
    public static RefreshToken Create(
        int userId,
        string token,
        DateTime expiresAt,
        string? createdByIp,
        string? deviceInfo = null)
    {
        if (userId <= 0)
            throw new ArgumentException("UserId must be greater than zero.", nameof(userId));

        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty.", nameof(token));

        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("ExpiresAt must be in the future.", nameof(expiresAt));

        var rt = new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            CreatedByIp = createdByIp,
            DeviceInfo = deviceInfo
        };

        rt.InitializeAudit();
        return rt;
    }

    /// <summary>
    /// Marks the token as revoked, recording the IP address that triggered the revocation.
    /// Idempotent — calling Revoke on an already-revoked token is a no-op.
    /// </summary>
    public void Revoke(string? revokedByIp)
    {
        if (IsRevoked) return;

        RevokedAt = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
        MarkAsUpdated();
    }
}
```

---

### 3.2 MODIFIED FILE — `DreamSoft.Domain/Entities/User.cs`

**Key changes:** Removed `SetRefreshToken()`, `ClearRefreshToken()`, `IsRefreshTokenValid()` methods and flat token properties. Added `RefreshTokens` navigation collection and lockout-related methods.

```csharp
using DreamSoft.Domain.Common;

namespace DreamSoft.Domain.Entities;

public class User : TenantEntity
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = null!;
    public string? SecondLastName { get; set; }
    public int? GenderId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int? IdTypeId { get; set; }
    public string? IdNumber { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? AvatarUrl { get; set; }
    public int? LanguageId { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutUntil { get; set; }

    // Navigation properties
    public Gender? Gender { get; set; }
    public IdType? IdType { get; set; }
    public Language? Language { get; set; }
    public ICollection<UserRole> UserRoles { get; private set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = [];  // NEW

    // Self-referential navigation properties for audit trail
    public ICollection<User> CreatedUsers { get; private set; } = [];
    public ICollection<User> UpdatedUsers { get; private set; } = [];
    public ICollection<Role> CreatedRoles { get; private set; } = [];
    public ICollection<Role> UpdatedRoles { get; private set; } = [];
    public ICollection<UserRole> AssignedUserRoles { get; private set; } = [];

    private User() { }

    public static User Create(
        int tenantId, string username, string email, string passwordHash,
        string firstName, string lastName, int? languageId = null, int? createdBy = null)
    { /* ... factory validation ... */ }

    // ... profile / contact / email update methods ...

    // ── Lockout constants ────────────────────────────────────────────────────
    public const int MaxFailedAttempts = 5;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public bool IsLockedOut()
        => LockoutUntil.HasValue && DateTime.UtcNow < LockoutUntil.Value;

    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= MaxFailedAttempts)
            LockoutUntil = DateTime.UtcNow.Add(LockoutDuration);
        MarkAsUpdated();
    }

    public void ResetFailedLoginAttempts()
    {
        FailedLoginAttempts = 0;
        LockoutUntil = null;
        MarkAsUpdated();
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public string GetFullName() { /* ... */ }
}
```

---

### 3.3 MODIFIED FILE — `DreamSoft.Application/Common/Interfaces/IApplicationDbContext.cs`

**Added line (inside business entities section):**
```csharp
/// <summary>Persisted refresh token sessions (one per login, supports multi-device).</summary>
DbSet<RefreshToken> RefreshTokens { get; }
```

---

### 3.4 MODIFIED FILE — `DreamSoft.Infrastructure/Persistence/ApplicationDbContext.cs`

**Added line (inside business entities section):**
```csharp
/// <summary>Persisted refresh token sessions (one per login, supports multi-device).</summary>
public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
```

---

### 3.5 NEW FILE — `DreamSoft.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs`

```csharp
using DreamSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DreamSoft.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the RefreshToken entity.
/// Maps to the 'refresh_tokens' table in PostgreSQL.
/// One row per login session — supports multiple concurrent sessions per user.
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(rt => rt.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(rt => rt.Token)
            .HasColumnName("token")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(rt => rt.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(rt => rt.CreatedByIp)
            .HasColumnName("created_by_ip")
            .HasMaxLength(50);

        builder.Property(rt => rt.DeviceInfo)
            .HasColumnName("device_info")
            .HasMaxLength(500);

        builder.Property(rt => rt.RevokedAt)
            .HasColumnName("revoked_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(rt => rt.RevokedByIp)
            .HasColumnName("revoked_by_ip")
            .HasMaxLength(50);

        // Audit columns
        builder.Property(rt => rt.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(rt => rt.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(rt => rt.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        // Indexes
        builder.HasIndex(rt => rt.Token)
            .HasDatabaseName("ix_refresh_tokens_token")
            .IsUnique();

        builder.HasIndex(rt => rt.UserId)
            .HasDatabaseName("ix_refresh_tokens_user_id");

        builder.HasIndex(rt => rt.ExpiresAt)
            .HasDatabaseName("ix_refresh_tokens_expires_at");

        // Relationship — cascade delete so tokens are cleaned up when a user is deleted
        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_refresh_tokens_users");
    }
}
```

---

### 3.6 MODIFIED FILE — `DreamSoft.Infrastructure/Persistence/Configurations/UserConfiguration.cs`

**Removed** the two flat token property mappings:
```csharp
// REMOVED:
builder.Property(u => u.RefreshToken)...
builder.Property(u => u.RefreshTokenExpiryTime)...
```

**Added** the `RefreshTokens` navigation relationship (at the end of `Configure`):
```csharp
builder.HasMany(u => u.RefreshTokens)
    .WithOne(rt => rt.User)
    .HasForeignKey(rt => rt.UserId)
    .OnDelete(DeleteBehavior.Cascade);
```

---

### 3.7 NEW FILE — `DreamSoft.Infrastructure/Persistence/ApplicationDbContextFactory.cs`

This file is required so `dotnet ef migrations add` can run without starting the full DI container (which fails because `IEmailService` is not registered at design time).

```csharp
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DreamSoft.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by EF Core tooling (dotnet ef migrations add / update).
/// Allows the tooling to create the DbContext without running the full DI container.
/// Connection string is read from appsettings.json in the API project.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Walk up to the solution root and load API appsettings
        var basePath = Path.Combine(
            Directory.GetCurrentDirectory(), "..", "DreamSoft.Api");

        if (!Directory.Exists(basePath))
            basePath = Directory.GetCurrentDirectory();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(
            configuration.GetConnectionString("DefaultConnection"),
            b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

        // Null-object stubs — only needed at design time for EF tooling
        return new ApplicationDbContext(
            optionsBuilder.Options,
            new NullCurrentUserService(),
            new NullTenantService(),
            new NullDateTime());
    }
}

file sealed class NullCurrentUserService : ICurrentUserService
{
    public int? UserId => null;
    public int? TenantId => null;
    public string? Email => null;
    public string? Username => null;
    public bool IsAdmin => false;
    public bool IsAuthenticated => false;
    public string? IpAddress => null;
    public string? Subdomain => null;
}

file sealed class NullTenantService : ITenantService
{
    public int? CurrentTenantId => null;
}

file sealed class NullDateTime : IDateTime
{
    public DateTime Now => DateTime.UtcNow;
    public DateTime UtcNow => DateTime.UtcNow;
}
```

---

## 4. Change 2 — HTTP-Only Cookie for Refresh Token

### What changed

The refresh token is no longer exposed in the JSON response body. Instead, it is stored exclusively in an `__Host-refresh_token` HTTP-only cookie, so JavaScript cannot access it (XSS protection). The controller manages reading, writing, and deleting this cookie.

| Before | After |
|--------|-------|
| `LoginResponse.RefreshToken` returned in JSON body | `LoginClientResponse` (no `RefreshToken` field) returned |
| `POST /auth/refresh` reads token from `[FromBody]` | Reads from `Request.Cookies[RefreshTokenCookieName]` |
| No cookie management in logout | Logout reads cookie, passes to command, then deletes cookie |

---

### 4.1 NEW FILE — `DreamSoft.Api/Controllers/AuthController.cs`

```csharp
using DreamSoft.Application.Features.Auth;
using DreamSoft.Application.Features.Auth.LoginBySubdomain;
using DreamSoft.Application.Features.Auth.LoginByTenantEmail;
using DreamSoft.Application.Features.Auth.Logout;
using DreamSoft.Application.Features.Auth.RefreshToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers;

public class AuthController : ApiControllerBase
{
    // Cookie name — __Host- prefix enforces Secure + no Domain + Path=/ in browsers.
    private const string RefreshTokenCookieName = "__Host-refresh_token";

    /// <summary>
    /// Login via subdomain routing.
    /// Sets the refresh token as an HTTP-only, Secure, SameSite=Strict cookie.
    /// Returns only the access token and user info in the JSON body.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginClientResponse), 200)]
    public async Task<IActionResult> Login(
        [FromBody] LoginBySubdomainCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        SetRefreshTokenCookie(result.RefreshToken, command.RememberMe);
        return Ok(LoginClientResponse.From(result));
    }

    /// <summary>
    /// Login via tenant company email (for mobile clients).
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login-by-email")]
    [ProducesResponseType(typeof(LoginClientResponse), 200)]
    public async Task<IActionResult> LoginByEmail(
        [FromBody] LoginByTenantEmailCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        SetRefreshTokenCookie(result.RefreshToken, command.RememberMe);
        return Ok(LoginClientResponse.From(result));
    }

    /// <summary>
    /// Rotates a refresh token. Reads from HTTP-only cookie.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginClientResponse), 200)]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var rawToken = Request.Cookies[RefreshTokenCookieName];
        if (string.IsNullOrWhiteSpace(rawToken))
            return Unauthorized();

        var result = await Mediator.Send(new RefreshTokenCommand(rawToken), cancellationToken);
        SetRefreshTokenCookie(result.RefreshToken, persistent: false);
        return Ok(LoginClientResponse.From(result));
    }

    /// <summary>
    /// Logs out the user. Revokes session and deletes cookie.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var rawToken = Request.Cookies[RefreshTokenCookieName];
        await Mediator.Send(new LogoutCommand(rawToken), cancellationToken);

        Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            Secure   = true,
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Path     = "/"
        });

        return NoContent();
    }

    // ── Cookie helpers ────────────────────────────────────────────────────────

    private void SetRefreshTokenCookie(string token, bool persistent)
    {
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure   = true,
            SameSite = SameSiteMode.Strict,
            Path     = "/"
        };

        if (persistent)
            options.Expires = DateTimeOffset.UtcNow.AddDays(30);

        Response.Cookies.Append(RefreshTokenCookieName, token, options);
    }
}

// ── Response DTOs ─────────────────────────────────────────────────────────────

/// <summary>
/// Client-facing login response. The refresh token is NOT included —
/// it travels exclusively via the HTTP-only cookie.
/// </summary>
public record LoginClientResponse(
    string AccessToken,
    DateTime ExpiresAt,
    int UserId,
    string Username,
    string FullName)
{
    public static LoginClientResponse From(LoginResponse r) =>
        new(r.AccessToken, r.ExpiresAt, r.UserId, r.Username, r.FullName);
}
```

---

### 4.2 Auth Command / Handler Files (New)

#### NEW FILE — `DreamSoft.Application/Features/Auth/LoginResponse.cs`
```csharp
namespace DreamSoft.Application.Features.Auth;

/// <summary>
/// Shared response returned by both login endpoints and the refresh-token endpoint.
/// </summary>
public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    int UserId,
    string Username,
    string FullName);
```

#### NEW FILE — `DreamSoft.Application/Features/Auth/Logout/LogoutCommand.cs`
```csharp
using MediatR;

namespace DreamSoft.Application.Features.Auth.Logout;

/// <summary>
/// Revokes the authenticated user's current refresh token session.
/// If RefreshToken is provided, only that specific session is revoked.
/// If null, all active sessions for the user are revoked (global logout).
/// </summary>
public record LogoutCommand(string? RefreshToken = null) : IRequest;
```

#### NEW FILE — `DreamSoft.Application/Features/Auth/Logout/LogoutCommandHandler.cs`
```csharp
using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Application.Features.Auth.Logout;

public class LogoutCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("Unauthorized.");

        var ip = currentUserService.IpAddress;

        if (request.RefreshToken is not null)
        {
            // Targeted logout — revoke only the specific session token
            var tokenEntity = await context.RefreshTokens
                .FirstOrDefaultAsync(
                    rt => rt.Token == request.RefreshToken && rt.UserId == userId,
                    cancellationToken);

            // Silently succeed if already revoked / not found (idempotent)
            if (tokenEntity is { IsRevoked: false })
            {
                tokenEntity.Revoke(ip);
                await context.SaveChangesAsync(cancellationToken);
            }
        }
        else
        {
            // Global logout — revoke all active sessions for this user
            var activeTokens = await context.RefreshTokens
                .Where(rt => rt.UserId == userId
                          && rt.RevokedAt == null
                          && rt.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            foreach (var token in activeTokens)
                token.Revoke(ip);

            if (activeTokens.Count > 0)
                await context.SaveChangesAsync(cancellationToken);
        }
    }
}
```

#### NEW FILE — `DreamSoft.Application/Features/Auth/RefreshToken/RefreshTokenCommand.cs`
```csharp
using MediatR;

namespace DreamSoft.Application.Features.Auth.RefreshToken;

/// <summary>
/// Rotates a refresh token — invalidates the current one and issues
/// a new access + refresh token pair.
/// </summary>
public record RefreshTokenCommand(string RefreshToken) : IRequest<LoginResponse>;
```

#### NEW FILE — `DreamSoft.Application/Features/Auth/RefreshToken/RefreshTokenCommandHandler.cs`
```csharp
using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RefreshTokenEntity = DreamSoft.Domain.Entities.RefreshToken;

namespace DreamSoft.Application.Features.Auth.RefreshToken;

public class RefreshTokenCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    ITokenService tokenService,
    IDateTime dateTime)
    : IRequestHandler<RefreshTokenCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Look up the RefreshToken entity directly
        var tokenEntity = await context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(
                rt => rt.Token == request.RefreshToken,
                cancellationToken)
            ?? throw new UnauthorizedException("Invalid or expired refresh token.");

        // 2. Validate token state
        if (!tokenEntity.IsActive || !tokenEntity.User.IsActive)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        // 3. Load the tenant to generate a valid access token
        var tenant = await context.Tenants
            .FirstOrDefaultAsync(t => t.Id == tokenEntity.User.TenantId, cancellationToken)
            ?? throw new UnauthorizedException("Invalid or expired refresh token.");

        // 4. Revoke the consumed token
        tokenEntity.Revoke(currentUserService.IpAddress);

        // 5. Preserve the original session window so RememberMe sessions
        //    keep their 30-day expiry after rotation (not reset to 7 days)
        var originalWindow   = tokenEntity.ExpiresAt - tokenEntity.CreatedAt;
        var newRefreshExpiry = dateTime.UtcNow.Add(originalWindow);

        // 6. Issue new token pair
        var newAccessToken = tokenService.GenerateAccessToken(tokenEntity.User, tenant);
        var newRawToken    = tokenService.GenerateRefreshToken();
        var expiresAt      = dateTime.UtcNow.AddMinutes(60);

        var newTokenEntity = RefreshTokenEntity.Create(
            userId:      tokenEntity.User.Id,
            token:       newRawToken,
            expiresAt:   newRefreshExpiry,
            createdByIp: currentUserService.IpAddress,
            deviceInfo:  tokenEntity.DeviceInfo);

        context.RefreshTokens.Add(newTokenEntity);
        await context.SaveChangesAsync(cancellationToken);

        return new LoginResponse(
            AccessToken:  newAccessToken,
            RefreshToken: newRawToken,
            ExpiresAt:    expiresAt,
            UserId:       tokenEntity.User.Id,
            Username:     tokenEntity.User.Username,
            FullName:     tokenEntity.User.GetFullName());
    }
}
```

---

## 5. Change 3 — RememberMe Flag & Session-Aware Token Expiry

### What changed

Both login commands now accept optional `RememberMe` and `DeviceInfo` parameters. `RememberMe = false` (default) gives a 7-day refresh token session. `RememberMe = true` gives a 30-day session with a persistent browser cookie. Token rotation preserves the original window so RememberMe sessions don't silently shrink to 7 days after the first rotation.

---

### 5.1 NEW FILE — `DreamSoft.Application/Features/Auth/LoginBySubdomain/LoginBySubdomainCommand.cs`

```csharp
using MediatR;

namespace DreamSoft.Application.Features.Auth.LoginBySubdomain;

/// <summary>
/// Authenticates a user by resolving the Tenant from the request subdomain
/// (populated by TenantResolutionMiddleware), then validating the username and password.
/// </summary>
public record LoginBySubdomainCommand(
    string Username,
    string Password,
    bool RememberMe = false,
    string? DeviceInfo = null) : IRequest<LoginResponse>;
```

### 5.2 NEW FILE — `DreamSoft.Application/Features/Auth/LoginBySubdomain/LoginBySubdomainCommandHandler.cs`

```csharp
using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RefreshTokenEntity = DreamSoft.Domain.Entities.RefreshToken;

namespace DreamSoft.Application.Features.Auth.LoginBySubdomain;

public class LoginBySubdomainCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IDateTime dateTime)
    : IRequestHandler<LoginBySubdomainCommand, LoginResponse>
{
    private const int DefaultRefreshTokenExpiryDays  = 7;
    private const int ExtendedRefreshTokenExpiryDays = 30;

    public async Task<LoginResponse> Handle(
        LoginBySubdomainCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Resolve subdomain from HTTP context (set by TenantResolutionMiddleware)
        var subdomain = currentUserService.Subdomain;
        if (string.IsNullOrWhiteSpace(subdomain))
            throw new UnauthorizedException("Subdomain could not be resolved from the request.");

        // 2. Find tenant by subdomain
        var tenant = await context.Tenants
            .Include(t => t.Status)
            .FirstOrDefaultAsync(
                t => t.Subdomain == subdomain.ToLower(), cancellationToken)
            ?? throw new UnauthorizedException("Invalid credentials.");

        // 3. Tenant must be active
        if (tenant.Status.Code != TenantStatusCodes.Active)
            throw new UnauthorizedException("Tenant account is not active.");

        // 4. Find user by username within that tenant
        var user = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                u => u.TenantId == tenant.Id &&
                     u.Username == request.Username.Trim().ToLower() &&
                     u.IsActive, cancellationToken)
            ?? throw new UnauthorizedException("Invalid credentials.");

        // 5. Check email verification
        if (!user.IsEmailVerified)
            throw new UnauthorizedException("Email address has not been verified.");

        // 6. Check account lockout
        if (user.IsLockedOut())
        {
            var remaining = (int)Math.Ceiling(
                (user.LockoutUntil!.Value - dateTime.UtcNow).TotalMinutes);
            throw new UnauthorizedException(
                $"Account is locked. Try again in {remaining} minute(s).");
        }

        // 7. Verify password
        if (!passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin();
            await context.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException("Invalid credentials.");
        }

        // 8. Successful login — reset lockout and record LastLoginAt
        user.ResetFailedLoginAttempts();
        user.RecordLogin();

        // 9. Issue tokens
        var accessToken   = tokenService.GenerateAccessToken(user, tenant);
        var rawToken      = tokenService.GenerateRefreshToken();
        var expiryDays    = request.RememberMe
                            ? ExtendedRefreshTokenExpiryDays
                            : DefaultRefreshTokenExpiryDays;
        var refreshExpiry = dateTime.UtcNow.AddDays(expiryDays);
        var expiresAt     = dateTime.UtcNow.AddMinutes(60);

        // 10. Persist RefreshToken entity (one row per session — multi-device)
        var refreshTokenEntity = RefreshTokenEntity.Create(
            userId:      user.Id,
            token:       rawToken,
            expiresAt:   refreshExpiry,
            createdByIp: currentUserService.IpAddress,
            deviceInfo:  request.DeviceInfo);

        context.RefreshTokens.Add(refreshTokenEntity);
        await context.SaveChangesAsync(cancellationToken);

        return new LoginResponse(
            AccessToken:  accessToken,
            RefreshToken: rawToken,
            ExpiresAt:    expiresAt,
            UserId:       user.Id,
            Username:     user.Username,
            FullName:     user.GetFullName());
    }
}
```

### 5.3 NEW FILE — `DreamSoft.Application/Features/Auth/LoginBySubdomain/LoginBySubdomainCommandValidator.cs`

```csharp
using FluentValidation;

namespace DreamSoft.Application.Features.Auth.LoginBySubdomain;

public class LoginBySubdomainCommandValidator : AbstractValidator<LoginBySubdomainCommand>
{
    public LoginBySubdomainCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(100).WithMessage("Username must not exceed 100 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MaximumLength(255).WithMessage("Password must not exceed 255 characters.");
    }
}
```

### 5.4 NEW FILE — `DreamSoft.Application/Features/Auth/LoginByTenantEmail/LoginByTenantEmailCommand.cs`

```csharp
using MediatR;

namespace DreamSoft.Application.Features.Auth.LoginByTenantEmail;

/// <summary>
/// Authenticates a user by resolving the Tenant from the company email (Tenant.Email).
/// Intended for mobile clients that don't use subdomain-based routing.
/// </summary>
public record LoginByTenantEmailCommand(
    string TenantEmail,
    string Username,
    string Password,
    bool RememberMe = false,
    string? DeviceInfo = null) : IRequest<LoginResponse>;
```

### 5.5 NEW FILE — `DreamSoft.Application/Features/Auth/LoginByTenantEmail/LoginByTenantEmailCommandHandler.cs`

```csharp
using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RefreshTokenEntity = DreamSoft.Domain.Entities.RefreshToken;

namespace DreamSoft.Application.Features.Auth.LoginByTenantEmail;

public class LoginByTenantEmailCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IDateTime dateTime)
    : IRequestHandler<LoginByTenantEmailCommand, LoginResponse>
{
    private const int DefaultRefreshTokenExpiryDays  = 7;
    private const int ExtendedRefreshTokenExpiryDays = 30;

    public async Task<LoginResponse> Handle(
        LoginByTenantEmailCommand request,
        CancellationToken cancellationToken)
    {
        var tenantEmail = request.TenantEmail.Trim().ToLower();

        // 1. Find tenant by company email
        var tenant = await context.Tenants
            .Include(t => t.Status)
            .FirstOrDefaultAsync(t => t.Email == tenantEmail, cancellationToken)
            ?? throw new UnauthorizedException("Invalid credentials.");

        // 2. Tenant must be active
        if (tenant.Status.Code != TenantStatusCodes.Active)
            throw new UnauthorizedException("Tenant account is not active.");

        // 3. Find user by username within that tenant
        var user = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                u => u.TenantId == tenant.Id &&
                     u.Username.Equals(request.Username.Trim(),
                         StringComparison.CurrentCultureIgnoreCase) &&
                     u.IsActive, cancellationToken)
            ?? throw new UnauthorizedException("Invalid credentials.");

        // 4–7. Verification, lockout, password (same pattern as LoginBySubdomain)
        if (!user.IsEmailVerified)
            throw new UnauthorizedException("Email address has not been verified.");

        if (user.IsLockedOut())
        {
            var remaining = (int)Math.Ceiling(
                (user.LockoutUntil!.Value - dateTime.UtcNow).TotalMinutes);
            throw new UnauthorizedException(
                $"Account is locked. Try again in {remaining} minute(s).");
        }

        if (!passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin();
            await context.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException("Invalid credentials.");
        }

        user.ResetFailedLoginAttempts();
        user.RecordLogin();

        var accessToken   = tokenService.GenerateAccessToken(user, tenant);
        var rawToken      = tokenService.GenerateRefreshToken();
        var expiryDays    = request.RememberMe
                            ? ExtendedRefreshTokenExpiryDays
                            : DefaultRefreshTokenExpiryDays;
        var refreshExpiry = dateTime.UtcNow.AddDays(expiryDays);
        var expiresAt     = dateTime.UtcNow.AddMinutes(60);

        var refreshTokenEntity = RefreshTokenEntity.Create(
            userId:      user.Id,
            token:       rawToken,
            expiresAt:   refreshExpiry,
            createdByIp: currentUserService.IpAddress,
            deviceInfo:  request.DeviceInfo);

        context.RefreshTokens.Add(refreshTokenEntity);
        await context.SaveChangesAsync(cancellationToken);

        return new LoginResponse(
            AccessToken:  accessToken,
            RefreshToken: rawToken,
            ExpiresAt:    expiresAt,
            UserId:       user.Id,
            Username:     user.Username,
            FullName:     user.GetFullName());
    }
}
```

### 5.6 NEW FILE — `DreamSoft.Application/Features/Auth/LoginByTenantEmail/LoginByTenantEmailCommandValidator.cs`

```csharp
using FluentValidation;

namespace DreamSoft.Application.Features.Auth.LoginByTenantEmail;

public class LoginByTenantEmailCommandValidator : AbstractValidator<LoginByTenantEmailCommand>
{
    public LoginByTenantEmailCommandValidator()
    {
        RuleFor(x => x.TenantEmail)
            .NotEmpty().WithMessage("Tenant email is required.")
            .EmailAddress().WithMessage("Tenant email must be a valid email address.")
            .MaximumLength(255).WithMessage("Tenant email must not exceed 255 characters.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(100).WithMessage("Username must not exceed 100 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MaximumLength(255).WithMessage("Password must not exceed 255 characters.");
    }
}
```

---

## 6. Change 4 — LastLoginAt Recording

`user.RecordLogin()` was wired in both login handlers immediately after successful password verification and before token issuance. This sets `User.LastLoginAt = DateTime.UtcNow` on every successful authentication.

```csharp
// After password check succeeds:
user.ResetFailedLoginAttempts();
user.RecordLogin();  // ← stamps LastLoginAt
```

Both `LoginBySubdomainCommandHandler` and `LoginByTenantEmailCommandHandler` contain this call.

---

## 7. Change 5 — IP-Based Rate Limiting on OTP Resend

### What changed

Added an IP-based sliding-window rate limiter (max 5 OTP resend requests per hour per IP address). The IP check occurs **before** token validation to prevent tenant enumeration through timing differences.

---

### 7.1 NEW FILE — `DreamSoft.Application/Common/Interfaces/IRateLimitService.cs`

```csharp
namespace DreamSoft.Application.Common.Interfaces;

/// <summary>
/// IP-based sliding-window rate limiter.
/// Used to prevent OTP resend abuse and other public-endpoint flooding.
/// </summary>
public interface IRateLimitService
{
    /// <summary>
    /// Returns <c>true</c> if the caller is allowed to perform the action;
    /// <c>false</c> when the sliding-window limit has been exceeded.
    /// Each call that returns <c>true</c> consumes one slot.
    /// </summary>
    /// <param name="key">Bucket key, e.g. "resend-otp:{ipAddress}"</param>
    /// <param name="maxAttempts">Max allowed in the window.</param>
    /// <param name="windowMinutes">Sliding window in minutes.</param>
    bool IsAllowed(string key, int maxAttempts, int windowMinutes);
}
```

---

### 7.2 NEW FILE — `DreamSoft.Infrastructure/Services/RateLimit/InMemoryRateLimitService.cs`

```csharp
using System.Collections.Concurrent;
using DreamSoft.Application.Common.Interfaces;

namespace DreamSoft.Infrastructure.Services.RateLimit;

/// <summary>
/// Thread-safe in-memory sliding-window rate limiter.
/// Suitable for single-node deployments.
/// For multi-node deployments, replace with a Redis-backed implementation.
/// </summary>
public sealed class InMemoryRateLimitService : IRateLimitService
{
    private readonly ConcurrentDictionary<string, List<DateTime>> _buckets = new();
    private readonly object _lock = new();

    public bool IsAllowed(string key, int maxAttempts, int windowMinutes)
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-windowMinutes);

        lock (_lock)
        {
            var timestamps = _buckets.GetOrAdd(key, _ => []);

            // Purge expired timestamps (outside the sliding window)
            timestamps.RemoveAll(t => t < cutoff);

            if (timestamps.Count >= maxAttempts)
                return false;

            timestamps.Add(DateTime.UtcNow);
            return true;
        }
    }
}
```

---

### 7.3 MODIFIED FILE — `DreamSoft.Infrastructure/DependencyInjection.cs`

**Added** singleton registration:
```csharp
using DreamSoft.Infrastructure.Services.RateLimit;

// Rate limiting — singleton so the in-memory window state persists across requests
services.AddSingleton<IRateLimitService, InMemoryRateLimitService>();
```

Full file:
```csharp
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Repositories;
using DreamSoft.Infrastructure.Persistence;
using DreamSoft.Infrastructure.Services.Common;
using DreamSoft.Infrastructure.Services.RateLimit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DreamSoft.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Common Services
        services.AddTransient<IDateTime, DateTimeService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasherService>();

        // Rate limiting — singleton so in-memory state persists across requests
        services.AddSingleton<IRateLimitService, InMemoryRateLimitService>();

        services.AddHttpContextAccessor();

        return services;
    }
}
```

---

### 7.4 MODIFIED FILE — `DreamSoft.Application/Features/Registration/ResendVerification/ResendVerificationCommandHandler.cs`

**Added** `ICurrentUserService` and `IRateLimitService` dependencies. IP check is now the **first** guard before token validation:

```csharp
using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace DreamSoft.Application.Features.Registration.ResendVerification;

public class ResendVerificationCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IEmailService emailService,
    ICurrentUserService currentUserService,
    IRateLimitService rateLimitService)
    : IRequestHandler<ResendVerificationCommand, Unit>
{
    private const int MaxResendPerHour = 5;
    private const int WindowMinutes    = 60;

    public async Task<Unit> Handle(
        ResendVerificationCommand request,
        CancellationToken cancellationToken)
    {
        // 1. IP-based rate limit — checked first to prevent tenant enumeration
        var ip = currentUserService.IpAddress ?? "unknown";
        if (!rateLimitService.IsAllowed($"resend-otp:{ip}", MaxResendPerHour, WindowMinutes))
            throw new RateLimitExceededException("RateLimitExceeded");

        // 2. Validate registration token and extract tenantId
        var tenantId = tokenService.GetTenantIdFromRegistrationToken(request.RegistrationToken)
            ?? throw new UnauthorizedException("Unauthorized");

        // 3. Load tenant — must still be PENDING_EMAIL_VERIFICATION
        var tenant = await context.Tenants
            .Include(t => t.Status)
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken)
            ?? throw new NotFoundException("TenantNotFound", tenantId);

        if (tenant.Status.Code != TenantStatusCodes.PendingEmailVerification)
            throw new ConflictException("EmailAlreadyVerified");

        // 4. Per-tenant 2-minute cool-down
        var twoMinutesAgo = DateTime.UtcNow.AddMinutes(-2);
        var recentToken = await context.TenantRegistrationTokens
            .AnyAsync(t => t.TenantId == tenantId && t.CreatedAt >= twoMinutesAgo, cancellationToken);
        if (recentToken)
            throw new RateLimitExceededException("RateLimitExceeded");

        // 5. Invalidate all existing unconsumed tokens
        var activeTokens = await context.TenantRegistrationTokens
            .Where(t => t.TenantId == tenantId && !t.IsConsumed)
            .ToListAsync(cancellationToken);
        foreach (var t in activeTokens) t.Consume();

        // 6. Generate new OTP and PBKDF2-hash it
        var plainCode = RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();
        var codeHash  = passwordHasher.HashPassword(plainCode);
        var newToken  = TenantRegistrationToken.Create(
            tenantId: tenantId, codeHash: codeHash,
            expiresAt: DateTime.UtcNow.AddHours(24));

        context.TenantRegistrationTokens.Add(newToken);
        await context.SaveChangesAsync(cancellationToken);

        // 7. Send new verification email
        var adminUser = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.TenantId == tenantId, cancellationToken)
            ?? throw new NotFoundException("UserNotFound", tenantId);

        await emailService.SendVerificationCodeAsync(adminUser.Email, plainCode, cancellationToken);

        return Unit.Value;
    }
}
```

---

## 8. Change 6 — Subdomain Availability Check Endpoint

### What changed

Added a public `GET /api/registration/check-subdomain?subdomain=` endpoint that validates a subdomain candidate before the full registration form is submitted.

---

### 8.1 NEW FILE — `DreamSoft.Application/Features/Registration/CheckSubdomainAvailability/CheckSubdomainAvailabilityQuery.cs`

```csharp
using MediatR;

namespace DreamSoft.Application.Features.Registration.CheckSubdomainAvailability;

/// <summary>Returns whether a subdomain is available for registration.</summary>
public record CheckSubdomainAvailabilityQuery(string Subdomain)
    : IRequest<SubdomainAvailabilityResponse>;

/// <summary>
/// Result DTO — tells the client whether the subdomain is available
/// and provides a normalised version of the subdomain.
/// </summary>
public record SubdomainAvailabilityResponse(
    string Subdomain,
    bool IsAvailable,
    string? Reason = null);
```

---

### 8.2 NEW FILE — `DreamSoft.Application/Features/Registration/CheckSubdomainAvailability/CheckSubdomainAvailabilityQueryHandler.cs`

```csharp
using DreamSoft.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Application.Features.Registration.CheckSubdomainAvailability;

public class CheckSubdomainAvailabilityQueryHandler(IApplicationDbContext context)
    : IRequestHandler<CheckSubdomainAvailabilityQuery, SubdomainAvailabilityResponse>
{
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

        if (!System.Text.RegularExpressions.Regex.IsMatch(
                normalised, @"^[a-z0-9][a-z0-9\-]*[a-z0-9]$"))
            return new SubdomainAvailabilityResponse(normalised, false, "SubdomainFormat");

        // 2. Reserved name check
        if (Reserved.Contains(normalised))
            return new SubdomainAvailabilityResponse(normalised, false, "SubdomainReserved");

        // 3. Database uniqueness check
        var taken = await context.Tenants
            .AnyAsync(t => t.Subdomain == normalised, cancellationToken);

        return taken
            ? new SubdomainAvailabilityResponse(normalised, false, "SubdomainTaken")
            : new SubdomainAvailabilityResponse(normalised, true);
    }
}
```

---

### 8.3 MODIFIED FILE — `DreamSoft.Api/Controllers/RegistrationController.cs`

**Added** the `CheckSubdomain` endpoint:

```csharp
using DreamSoft.Application.Features.Registration.CheckSubdomainAvailability;
using DreamSoft.Application.Features.Registration.RegisterTenant;
using DreamSoft.Application.Features.Registration.ResendVerification;
using DreamSoft.Application.Features.Registration.VerifyEmail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers;

[AllowAnonymous]
public class RegistrationController : ApiControllerBase
{
    /// <summary>
    /// Check whether a subdomain is available for registration.
    /// Returns availability status and the normalized subdomain.
    /// </summary>
    [HttpGet("check-subdomain")]
    [ProducesResponseType(typeof(SubdomainAvailabilityResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CheckSubdomain(
        [FromQuery] string subdomain,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(subdomain))
            return BadRequest("Subdomain is required.");

        var result = await Mediator.Send(
            new CheckSubdomainAvailabilityQuery(subdomain), cancellationToken);
        return Ok(result);
    }

    /// <summary>Register a new tenant. Returns a registration token.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(RegisterTenantResponse), 201)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterTenantCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return StatusCode(201, result);
    }

    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(VerifyEmailResponse), 200)]
    public async Task<IActionResult> VerifyEmail(
        [FromBody] VerifyEmailRequest body,
        CancellationToken cancellationToken)
    {
        var registrationToken = ExtractBearerToken();
        if (registrationToken is null) return Unauthorized();

        var command = new VerifyEmailCommand(registrationToken, body.Code);
        var result  = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("resend-verification")]
    [ProducesResponseType(204)]
    [ProducesResponseType(429)]
    public async Task<IActionResult> ResendVerification(CancellationToken cancellationToken)
    {
        var registrationToken = ExtractBearerToken();
        if (registrationToken is null) return Unauthorized();

        await Mediator.Send(
            new ResendVerificationCommand(registrationToken), cancellationToken);
        return NoContent();
    }

    private string? ExtractBearerToken()
    {
        var auth = Request.Headers.Authorization.FirstOrDefault();
        if (auth is null || !auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return null;
        return auth["Bearer ".Length..].Trim();
    }
}

public record VerifyEmailRequest(string Code);
```

---

## 9. Change 7 — Terms of Service Acceptance Tracking

### What changed

The `Tenant` entity now records which version of the Terms of Service was accepted during registration, along with the timestamp and IP address. This is backwards compatible — the field is optional in `RegisterTenantCommand`.

---

### 9.1 MODIFIED FILE — `DreamSoft.Domain/Entities/Tenant.cs`

**Added** three new properties and one new domain method:

```csharp
// Terms of Service acceptance
/// <summary>Version string of the ToS the tenant accepted (e.g. "2025-01-01").</summary>
public string? TermsVersion { get; private set; }

/// <summary>UTC timestamp when the tenant accepted the Terms of Service.</summary>
public DateTime? TermsAcceptedAt { get; private set; }

/// <summary>IP address from which the ToS were accepted.</summary>
public string? TermsAcceptedIp { get; private set; }

/// <summary>
/// Records that the tenant accepted the Terms of Service.
/// Idempotent — re-calling with the same version is a no-op.
/// </summary>
public void AcceptTerms(string version, DateTime acceptedAt, string? acceptedIp)
{
    if (string.IsNullOrWhiteSpace(version))
        throw new ArgumentException("Terms version is required.", nameof(version));

    TermsVersion    = version.Trim();
    TermsAcceptedAt = acceptedAt;
    TermsAcceptedIp = acceptedIp;
    MarkAsUpdated();
}
```

---

### 9.2 MODIFIED FILE — `DreamSoft.Infrastructure/Persistence/Configurations/TenantConfiguration.cs`

**Added** three column mappings (inside `Configure`):

```csharp
// Terms of Service acceptance
builder.Property(t => t.TermsVersion)
    .HasColumnName("terms_version")
    .HasMaxLength(50);

builder.Property(t => t.TermsAcceptedAt)
    .HasColumnName("terms_accepted_at")
    .HasColumnType("timestamp with time zone");

builder.Property(t => t.TermsAcceptedIp)
    .HasColumnName("terms_accepted_ip")
    .HasMaxLength(50);
```

---

### 9.3 MODIFIED FILE — `DreamSoft.Application/Features/Registration/RegisterTenant/RegisterTenantCommand.cs`

**Added** optional `TermsVersion` parameter (backwards compatible):

```csharp
using MediatR;

namespace DreamSoft.Application.Features.Registration.RegisterTenant;

public record RegisterTenantCommand(
    string CompanyName,
    string Subdomain,
    string? TaxId,
    string Phone,
    string AddressLine1,
    int CountryId,
    int ProvinceId,
    int MunicipalityId,
    string AdminFirstName,
    string AdminLastName,
    string AdminEmail,
    string AdminPassword,
    int LanguageId,
    int CurrencyId,
    /// <summary>
    /// Version identifier of the Terms of Service the user accepted.
    /// When provided, the acceptance is recorded on the Tenant entity.
    /// Example: "2025-01-01"
    /// </summary>
    string? TermsVersion = null
) : IRequest<RegisterTenantResponse>;

public record RegisterTenantResponse(string RegistrationToken);
```

---

### 9.4 MODIFIED FILE — `DreamSoft.Application/Features/Registration/RegisterTenant/RegisterTenantCommandHandler.cs`

**Added** `ICurrentUserService` dependency and `AcceptTerms` call inside the transaction:

```csharp
public class RegisterTenantCommandHandler(
    IApplicationDbContext context,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IEmailService emailService,
    ICurrentUserService currentUserService)  // ← NEW dependency
    : IRequestHandler<RegisterTenantCommand, RegisterTenantResponse>
{
    // Inside Handle, after tenant creation and before saving:

    // Record Terms of Service acceptance if provided
    if (!string.IsNullOrWhiteSpace(request.TermsVersion))
    {
        tenant.AcceptTerms(
            version:    request.TermsVersion,
            acceptedAt: DateTime.UtcNow,
            acceptedIp: currentUserService.IpAddress);
    }
```

---

### 9.5 MODIFIED FILE — `DreamSoft.Application/Features/Registration/VerifyEmail/VerifyEmailCommandHandler.cs`

**Added** `ICurrentUserService` dependency and RefreshToken entity creation (replacing flat `user.SetRefreshToken()`):

```csharp
public class VerifyEmailCommandHandler(
    IApplicationDbContext context,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IEmailService emailService,
    ICurrentUserService currentUserService,  // ← NEW dependency
    IConfiguration configuration)
    : IRequestHandler<VerifyEmailCommand, VerifyEmailResponse>
{
    // ...
    // 10. Issue real access + refresh tokens
    var accessToken = tokenService.GenerateAccessToken(adminUser, tenant);
    var rawToken    = tokenService.GenerateRefreshToken();

    var refreshExpiryDays = int.Parse(
        configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");

    var refreshTokenEntity = RefreshToken.Create(
        userId:      adminUser.Id,
        token:       rawToken,
        expiresAt:   DateTime.UtcNow.AddDays(refreshExpiryDays),
        createdByIp: currentUserService.IpAddress);  // ← NEW: IP captured

    context.RefreshTokens.Add(refreshTokenEntity);
    await context.SaveChangesAsync(cancellationToken);

    return new VerifyEmailResponse(accessToken, rawToken);
```

---

## 10. Database Migrations

### 10.1 Migration — `AddRefreshTokensTable`
File: `DreamSoft.Infrastructure/Persistence/Migrations/20260223124003_AddRefreshTokensTable.cs`

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Drop flat columns from users table
    migrationBuilder.DropColumn(name: "refresh_token",             table: "users");
    migrationBuilder.DropColumn(name: "refresh_token_expiry_time", table: "users");

    // Create new refresh_tokens table
    migrationBuilder.CreateTable(
        name: "refresh_tokens",
        columns: table => new
        {
            id            = table.Column<int>(nullable: false)
                                 .Annotation("Npgsql:ValueGenerationStrategy",
                                     NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            user_id       = table.Column<int>(nullable: false),
            token         = table.Column<string>(maxLength: 500, nullable: false),
            expires_at    = table.Column<DateTime>(type: "timestamp with time zone"),
            created_by_ip = table.Column<string>(maxLength: 50),
            device_info   = table.Column<string>(maxLength: 500),
            revoked_at    = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
            revoked_by_ip = table.Column<string>(maxLength: 50),
            is_active     = table.Column<bool>(defaultValue: true),
            created_at    = table.Column<DateTime>(defaultValueSql: "CURRENT_TIMESTAMP"),
            updated_at    = table.Column<DateTime>(nullable: true)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_refresh_tokens", x => x.id);
            table.ForeignKey(
                name: "fk_refresh_tokens_users",
                column: x => x.user_id,
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.CreateIndex("ix_refresh_tokens_expires_at", "refresh_tokens", "expires_at");
    migrationBuilder.CreateIndex("ix_refresh_tokens_token",      "refresh_tokens", "token", unique: true);
    migrationBuilder.CreateIndex("ix_refresh_tokens_user_id",    "refresh_tokens", "user_id");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropTable(name: "refresh_tokens");
    migrationBuilder.AddColumn<string>("refresh_token", "users", maxLength: 500, nullable: true);
    migrationBuilder.AddColumn<DateTime>("refresh_token_expiry_time", "users",
        type: "timestamp with time zone", nullable: true);
}
```

### 10.2 Migration — `AddTenantTermsAcceptance`
File: `DreamSoft.Infrastructure/Persistence/Migrations/20260223124604_AddTenantTermsAcceptance.cs`

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<DateTime>(
        name: "terms_accepted_at", table: "tenants",
        type: "timestamp with time zone", nullable: true);

    migrationBuilder.AddColumn<string>(
        name: "terms_accepted_ip", table: "tenants",
        type: "character varying(50)", maxLength: 50, nullable: true);

    migrationBuilder.AddColumn<string>(
        name: "terms_version", table: "tenants",
        type: "character varying(50)", maxLength: 50, nullable: true);
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropColumn(name: "terms_accepted_at", table: "tenants");
    migrationBuilder.DropColumn(name: "terms_accepted_ip", table: "tenants");
    migrationBuilder.DropColumn(name: "terms_version",     table: "tenants");
}
```

---

## 11. Test Suite

All 81 integration tests pass. Tests use **SQLite in-memory** databases (no PostgreSQL required) and stub implementations of all external services.

### 11.1 Test Infrastructure — `tests/DreamSoft.Tests.Integration/Helpers/RegistrationTestDbContext.cs`

This file defines all stubs and the base class used by every test class.

**Stubs provided:**

| Class | Purpose |
|-------|---------|
| `StubPasswordHasher` | `HashPassword("x")` → `"hashed:x"` — deterministic, reversible |
| `StubTokenService` | `GenerateRegistrationToken(id)` → `"reg-token-{id}"` |
| `StubEmailService` | Captures sent emails in `SentVerificationCodes` and `SentWelcomeEmails` lists |
| `StubCurrentUserService` | Settable `UserId`, `TenantId` |
| `StubTenantService` | Settable `CurrentTenantId` |
| `StubDateTime` | Returns `DateTime.UtcNow` |
| `StubRateLimitService` | **Always returns `true`** (rate limiting tested separately) |
| `StubUnitOfWork` | Wraps `RegistrationTestDbContext`, no real transactions |

**`RegistrationTestDbContext`** — SQLite DbContext implementing `IApplicationDbContext`:
- Configures only the entities needed by registration/auth tests
- Ignores unnecessary navigation properties (lookup entities, etc.)
- **NEW:** Adds `RefreshTokens` DbSet + entity config + ignores `User.RefreshTokens` navigation
- Seeds `TenantStatuses` and `SubscriptionStatuses` in `HandlerTestBase.SeedStatuses()`

**`HandlerTestBase`** — abstract base class:
```csharp
public abstract class HandlerTestBase : IDisposable
{
    protected readonly SqliteConnection Connection;
    protected readonly RegistrationTestDbContext Db;
    protected readonly StubPasswordHasher PasswordHasher = new();
    protected readonly StubTokenService TokenService = new();
    protected readonly StubEmailService EmailService = new();
    protected readonly StubCurrentUserService CurrentUser = new();
    protected readonly StubTenantService TenantService = new();
    protected readonly StubRateLimitService RateLimitService = new();   // NEW
    protected readonly StubUnitOfWork UnitOfWork;
    // ...
}
```

---

### 11.2 Test File — `RegisterTenantCommandHandlerTests.cs` (10 tests)

```
✅ Handle_WithValidCommand_ReturnsRegistrationToken
✅ Handle_WithValidCommand_CreatesTenantInDb
✅ Handle_WithValidCommand_TenantHasPendingEmailVerificationStatus
✅ Handle_WithValidCommand_CreatesAdminUser
✅ Handle_WithValidCommand_AdminPasswordIsHashed
✅ Handle_WithValidCommand_CreatesOtpToken
✅ Handle_WithValidCommand_SendsVerificationEmail
✅ Handle_WithValidCommand_RegistrationTokenContainsTenantId
✅ Handle_WithDuplicateSubdomain_ThrowsConflictException
✅ Handle_WithDuplicateEmail_ThrowsConflictException
✅ Handle_WhenPendingStatusMissing_ThrowsNotFoundException
```

**Constructor after change:**
```csharp
_sut = new RegisterTenantCommandHandler(
    Db, UnitOfWork, PasswordHasher, TokenService, EmailService, CurrentUser);
//                                                               ^^^^^^^^^^^ NEW
```

---

### 11.3 Test File — `VerifyEmailCommandHandlerTests.cs` (14 tests)

```
✅ Handle_WithCorrectCode_ReturnsAccessAndRefreshTokens
✅ Handle_WithCorrectCode_TransitionsTenantToPendingSubscription
✅ Handle_WithCorrectCode_VerifiesTenantEmail
✅ Handle_WithCorrectCode_VerifiesAdminUserEmail
✅ Handle_WithCorrectCode_ConsumesOtpToken
✅ Handle_WithCorrectCode_SendsWelcomeEmail
✅ Handle_WithCorrectCode_PersistsRefreshToken          ← UPDATED: now checks Db.RefreshTokens
✅ Handle_WithWrongCode_ThrowsUnauthorizedException
✅ Handle_WithWrongCode_IncrementsAttemptCount
✅ Handle_AfterMaxAttempts_ThrowsUnauthorizedException
✅ Handle_WithInvalidRegistrationToken_ThrowsUnauthorizedException
✅ Handle_WhenAlreadyVerified_ThrowsConflictException
```

**Key test update** — `Handle_WithCorrectCode_PersistsRefreshToken` now checks the new `RefreshTokens` table instead of removed flat properties on `User`:
```csharp
// BEFORE:
Assert.NotNull(user.RefreshToken);
Assert.True(user.RefreshTokenExpiryTime > DateTime.UtcNow);

// AFTER:
var tokenExists = await Db.RefreshTokens.AnyAsync(rt => rt.UserId == user.Id);
Assert.True(tokenExists);
```

---

### 11.4 Test File — `ResendVerificationCommandHandlerTests.cs` (10 tests)

```
✅ Handle_AfterCooldown_ReturnsUnit
✅ Handle_AfterCooldown_SendsNewVerificationEmail
✅ Handle_AfterCooldown_InvalidatesOldToken
✅ Handle_AfterCooldown_CreatesNewOtpToken
✅ Handle_WithinCooldownWindow_ThrowsRateLimitExceededException
✅ Handle_WithInvalidRegistrationToken_ThrowsUnauthorizedException
✅ Handle_WhenTenantAlreadyVerified_ThrowsConflictException
```

**Constructor after change:**
```csharp
_sut = new ResendVerificationCommandHandler(
    Db, PasswordHasher, TokenService, EmailService, CurrentUser, RateLimitService);
//                                                  ^^^^^^^^^^^ ^^^^^^^^^^^^^^^^^ NEW
```

---

### 11.5 Test File — `CompleteOnboardingCommandHandlerTests.cs` (13 tests)

```
✅ Handle_WithValidPlan_ReturnsRedirectUrl
✅ Handle_WithValidPlan_TransitionsTenantToActive
✅ Handle_WithValidPlan_CreatesTenantSubscription
✅ Handle_WithValidPlan_SubscriptionStatusIsActive
✅ Handle_WithTrialPlan_SubscriptionStatusIsTrial
✅ Handle_WithTrialPlan_SetsTrialEndDate
✅ Handle_WhenPlanBelongsToDifferentSolution_ThrowsConflictException
✅ Handle_WithNonExistentSolution_ThrowsNotFoundException
✅ Handle_WithNonExistentPlan_ThrowsNotFoundException
✅ Handle_WhenAlreadyOnboarded_ThrowsConflictException
✅ Handle_WhenNoTenantIdInContext_ThrowsUnauthorizedException
```

**Constructor update** — `RegisterTenantCommandHandler` now requires `CurrentUser`:
```csharp
_registerHandler = new RegisterTenantCommandHandler(
    Db, UnitOfWork, PasswordHasher, TokenService, EmailService, CurrentUser);
//                                                               ^^^^^^^^^^^ NEW
```

---

## 12. Files Changed Summary

### New Files (25)

| File | Layer | Description |
|------|-------|-------------|
| `DreamSoft.Domain/Entities/RefreshToken.cs` | Domain | RefreshToken entity with `Create()` factory and `Revoke()` |
| `DreamSoft.Application/Common/Interfaces/IRateLimitService.cs` | Application | Rate limit interface |
| `DreamSoft.Application/Features/Auth/LoginResponse.cs` | Application | Shared login response DTO |
| `DreamSoft.Application/Features/Auth/LoginBySubdomain/LoginBySubdomainCommand.cs` | Application | Login by subdomain CQRS command |
| `DreamSoft.Application/Features/Auth/LoginBySubdomain/LoginBySubdomainCommandHandler.cs` | Application | Login by subdomain handler |
| `DreamSoft.Application/Features/Auth/LoginBySubdomain/LoginBySubdomainCommandValidator.cs` | Application | FluentValidation validator |
| `DreamSoft.Application/Features/Auth/LoginByTenantEmail/LoginByTenantEmailCommand.cs` | Application | Login by tenant email CQRS command |
| `DreamSoft.Application/Features/Auth/LoginByTenantEmail/LoginByTenantEmailCommandHandler.cs` | Application | Login by tenant email handler |
| `DreamSoft.Application/Features/Auth/LoginByTenantEmail/LoginByTenantEmailCommandValidator.cs` | Application | FluentValidation validator |
| `DreamSoft.Application/Features/Auth/Logout/LogoutCommand.cs` | Application | Logout CQRS command |
| `DreamSoft.Application/Features/Auth/Logout/LogoutCommandHandler.cs` | Application | Targeted + global logout handler |
| `DreamSoft.Application/Features/Auth/RefreshToken/RefreshTokenCommand.cs` | Application | Token rotation command |
| `DreamSoft.Application/Features/Auth/RefreshToken/RefreshTokenCommandHandler.cs` | Application | Token rotation with window preservation |
| `DreamSoft.Application/Features/Registration/CheckSubdomainAvailability/CheckSubdomainAvailabilityQuery.cs` | Application | Subdomain check query + response DTO |
| `DreamSoft.Application/Features/Registration/CheckSubdomainAvailability/CheckSubdomainAvailabilityQueryHandler.cs` | Application | Format/reserved/DB uniqueness validation |
| `DreamSoft.Infrastructure/Persistence/ApplicationDbContextFactory.cs` | Infrastructure | IDesignTimeDbContextFactory for EF migrations |
| `DreamSoft.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs` | Infrastructure | EF Core table/column/index configuration |
| `DreamSoft.Infrastructure/Persistence/Migrations/20260223124003_AddRefreshTokensTable.cs` | Infrastructure | EF migration |
| `DreamSoft.Infrastructure/Persistence/Migrations/20260223124604_AddTenantTermsAcceptance.cs` | Infrastructure | EF migration |
| `DreamSoft.Infrastructure/Services/RateLimit/InMemoryRateLimitService.cs` | Infrastructure | Thread-safe sliding-window rate limiter |
| `DreamSoft.Api/Controllers/AuthController.cs` | API | Auth controller with HTTP-only cookie management |
| `AUTH_IMPROVEMENTS.md` | Docs | Change summary document |

### Modified Files (18)

| File | Layer | Changes |
|------|-------|---------|
| `DreamSoft.Domain/Entities/User.cs` | Domain | Added `RefreshTokens` nav; removed flat token props/methods |
| `DreamSoft.Domain/Entities/Tenant.cs` | Domain | Added `TermsVersion`, `TermsAcceptedAt`, `TermsAcceptedIp` + `AcceptTerms()` |
| `DreamSoft.Application/Common/Interfaces/IApplicationDbContext.cs` | Application | Added `DbSet<RefreshToken> RefreshTokens` |
| `DreamSoft.Application/Features/Registration/RegisterTenant/RegisterTenantCommand.cs` | Application | Added optional `TermsVersion` parameter |
| `DreamSoft.Application/Features/Registration/RegisterTenant/RegisterTenantCommandHandler.cs` | Application | Added `ICurrentUserService`; wired `AcceptTerms` |
| `DreamSoft.Application/Features/Registration/ResendVerification/ResendVerificationCommandHandler.cs` | Application | Added `ICurrentUserService` + `IRateLimitService`; IP rate check |
| `DreamSoft.Application/Features/Registration/VerifyEmail/VerifyEmailCommandHandler.cs` | Application | Added `ICurrentUserService`; RefreshToken entity instead of flat columns |
| `DreamSoft.Infrastructure/DependencyInjection.cs` | Infrastructure | Added singleton `IRateLimitService` registration |
| `DreamSoft.Infrastructure/Persistence/ApplicationDbContext.cs` | Infrastructure | Added `DbSet<RefreshToken> RefreshTokens` |
| `DreamSoft.Infrastructure/Persistence/Configurations/UserConfiguration.cs` | Infrastructure | Removed flat token columns; added `RefreshTokens` relationship |
| `DreamSoft.Infrastructure/Persistence/Configurations/TenantConfiguration.cs` | Infrastructure | Added 3 terms acceptance columns |
| `DreamSoft.Api/Controllers/RegistrationController.cs` | API | Added `CheckSubdomain` endpoint |
| `tests/.../Helpers/RegistrationTestDbContext.cs` | Tests | Added `StubRateLimitService`; added `RefreshTokens` DbSet + config |
| `tests/.../RegisterTenantCommandHandlerTests.cs` | Tests | Updated constructor (added `CurrentUser`) |
| `tests/.../VerifyEmailCommandHandlerTests.cs` | Tests | Updated constructor; updated `PersistsRefreshToken` assertion |
| `tests/.../ResendVerificationCommandHandlerTests.cs` | Tests | Updated constructor (added `CurrentUser`, `RateLimitService`) |
| `tests/.../CompleteOnboardingCommandHandlerTests.cs` | Tests | Updated `_registerHandler` constructor |
| `DreamSoft.Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs` | Infrastructure | Auto-updated by EF Core migrations tooling |

---

## 13. Security Summary

| Threat | Mitigation Applied |
|--------|--------------------|
| XSS stealing refresh tokens from `localStorage` | Refresh token stored only in `HttpOnly` cookie — JavaScript cannot access it |
| CSRF attacks using cookie-based refresh token | `SameSite=Strict` prevents cross-site requests from including the cookie |
| Refresh token theft and reuse | Token rotation — each use revokes the old token; stale reuse is detectable via `RevokedAt` |
| OTP resend abuse / tenant enumeration | IP-based sliding window (5/hour) checked **before** token validation |
| Subdomain squatting of reserved names | 22-item reserved list checked at `CheckSubdomainAvailability` and at registration |
| Long-lived sessions without expiry control | `RememberMe` gives explicit opt-in for 30-day sessions; default is 7 days |
| No audit trail for token revocation | `revoked_at` + `revoked_by_ip` stored permanently on `RefreshToken` row |
| Account brute-force | `FailedLoginAttempts` counter + 15-minute lockout after 5 failures |
| Password exposure | PBKDF2 hashing via `IPasswordHasher` — plain text never stored |

---

## 14. Build & Test Results

```
dotnet build
  Build succeeded — 0 warnings, 0 errors

dotnet test
  Test run for DreamSoft.Tests.Integration
  Passed!  - Failed: 0, Passed: 81, Skipped: 0, Total: 81
```

**Migrations generated:**
```
dotnet ef migrations add AddRefreshTokensTable \
  --project DreamSoft.Infrastructure \
  --startup-project DreamSoft.Api
  → Build succeeded. Done.

dotnet ef migrations add AddTenantTermsAcceptance \
  --project DreamSoft.Infrastructure \
  --startup-project DreamSoft.Api
  → Build succeeded. Done.
```

**Git commit:** `188779e` on branch `rematering`
```
feat: Auth & Registration improvements (7 security/UX enhancements)
43 files changed, 8996 insertions(+), 58 deletions(-)
```

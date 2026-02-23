# Auth & Registration Improvements

## Overview

This document summarises all changes made to the `rematering` branch as part of the auth-improvement implementation plan. Seven areas were addressed, covering security, multi-device support, auditability, rate-limiting, and UX.

---

## Change 1 — Separate `RefreshToken` Entity (Multi-Device Sessions + IP Audit Trail)

**Priority:** HIGH
**Files changed:**
- `DreamSoft.Domain/Entities/RefreshToken.cs` — **Created**
- `DreamSoft.Domain/Entities/User.cs` — **Modified**
- `DreamSoft.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs` — **Created**
- `DreamSoft.Infrastructure/Persistence/Configurations/UserConfiguration.cs` — **Modified**
- `DreamSoft.Application/Common/Interfaces/IApplicationDbContext.cs` — **Modified**
- `DreamSoft.Infrastructure/Persistence/ApplicationDbContext.cs` — **Modified**
- Migration: `AddRefreshTokensTable`

### What changed
| Before | After |
|--------|-------|
| Two flat columns on `users`: `refresh_token`, `refresh_token_expiry_time` | Separate `refresh_tokens` table — one row per login session |
| Single active token per user (last login overwrites previous) | Multiple concurrent sessions supported (multi-device) |
| No IP tracking on token creation/revocation | `created_by_ip`, `revoked_by_ip`, `device_info` columns |
| `User.SetRefreshToken()` / `ClearRefreshToken()` / `IsRefreshTokenValid()` methods | `RefreshToken.Create(…)` factory + `Revoke(ip)` domain method |

### `RefreshToken` entity key members
```csharp
public class RefreshToken : AuditableEntity
{
    public int UserId { get; private set; }
    public string Token { get; private set; }           // Cryptographically random opaque string
    public DateTime ExpiresAt { get; private set; }
    public string? CreatedByIp { get; private set; }
    public string? DeviceInfo { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedByIp { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public new bool IsActive => !IsRevoked && !IsExpired;  // shadows AuditableEntity.IsActive

    public static RefreshToken Create(int userId, string token, DateTime expiresAt,
        string? createdByIp, string? deviceInfo = null) { … }

    public void Revoke(string? revokedByIp) { … }  // idempotent
}
```

### Database schema (`refresh_tokens`)
| Column | Type | Notes |
|--------|------|-------|
| `id` | `integer` | PK, auto-increment |
| `user_id` | `integer` | FK → `users.id` (CASCADE DELETE) |
| `token` | `varchar(500)` | Unique index |
| `expires_at` | `timestamptz` | Index |
| `created_by_ip` | `varchar(50)` | |
| `device_info` | `varchar(500)` | User-Agent / device hint |
| `revoked_at` | `timestamptz` | NULL = active |
| `revoked_by_ip` | `varchar(50)` | |
| `is_active` | `boolean` | Audit field (default true) |
| `created_at` | `timestamptz` | Default CURRENT_TIMESTAMP |
| `updated_at` | `timestamptz` | |

---

## Change 2 — HTTP-Only Cookie for Refresh Token

**Priority:** HIGH
**Files changed:**
- `DreamSoft.Api/Controllers/AuthController.cs` — **Modified**

### What changed
| Before | After |
|--------|-------|
| Refresh token returned in JSON body (`LoginResponse.RefreshToken`) | Refresh token written to `__Host-refresh_token` HTTP-only cookie |
| `POST /auth/refresh` reads token from `[FromBody]` | `POST /auth/refresh` reads token from cookie (no body needed) |
| `POST /auth/logout` had no token parameter | `POST /auth/logout` reads cookie, revokes that session, then deletes cookie |
| `LoginResponse` record (with `RefreshToken`) returned to client | New `LoginClientResponse` (without `RefreshToken`) returned to client |

### Cookie properties
```
Name:     __Host-refresh_token
HttpOnly: true  — JavaScript cannot access it (XSS protection)
Secure:   true  — HTTPS only
SameSite: Strict — CSRF protection
Path:     /
Expires:  Session (default) or +30 days (RememberMe)
```

The `__Host-` prefix enforces `Secure`, no `Domain` attribute, and `Path=/` in browsers (additional browser-level hardening).

---

## Change 3 — `RememberMe` Flag + Session-Aware Token Expiry

**Priority:** MEDIUM
**Files changed:**
- `DreamSoft.Application/Features/Auth/LoginBySubdomain/LoginBySubdomainCommand.cs`
- `DreamSoft.Application/Features/Auth/LoginBySubdomain/LoginBySubdomainCommandHandler.cs`
- `DreamSoft.Application/Features/Auth/LoginByTenantEmail/LoginByTenantEmailCommand.cs`
- `DreamSoft.Application/Features/Auth/LoginByTenantEmail/LoginByTenantEmailCommandHandler.cs`
- `DreamSoft.Application/Features/Auth/RefreshToken/RefreshTokenCommandHandler.cs`

### What changed
| Before | After |
|--------|-------|
| Hardcoded 7-day refresh token expiry for all sessions | `RememberMe = false` → 7-day expiry; `RememberMe = true` → 30-day expiry |
| Token rotation reset expiry to a fixed 7 days | Token rotation **preserves the original session window** (30-day session stays at 30 days after rotation) |
| No `DeviceInfo` carried in requests | Optional `DeviceInfo` (User-Agent hint) stored on the `RefreshToken` row |

### Token rotation expiry preservation
```csharp
// Preserve the original session window
var originalWindow   = tokenEntity.ExpiresAt - tokenEntity.CreatedAt;
var newRefreshExpiry = dateTime.UtcNow.Add(originalWindow);
```

---

## Change 4 — `LastLoginAt` Recording

**Priority:** MEDIUM
**Files changed:**
- `DreamSoft.Application/Features/Auth/LoginBySubdomain/LoginBySubdomainCommandHandler.cs`
- `DreamSoft.Application/Features/Auth/LoginByTenantEmail/LoginByTenantEmailCommandHandler.cs`

### What changed
`user.RecordLogin()` was already present in the codebase but never wired correctly with the new entity-based flow. Both login handlers now explicitly call it before saving — ensuring `LastLoginAt` is always stamped on every successful authentication.

---

## Change 5 — IP-Based Rate Limiting on OTP Resend

**Priority:** MEDIUM
**Files changed:**
- `DreamSoft.Application/Common/Interfaces/IRateLimitService.cs` — **Created**
- `DreamSoft.Infrastructure/Services/RateLimit/InMemoryRateLimitService.cs` — **Created**
- `DreamSoft.Infrastructure/DependencyInjection.cs` — **Modified** (registered as Singleton)
- `DreamSoft.Application/Features/Registration/ResendVerification/ResendVerificationCommandHandler.cs` — **Modified**

### What changed
| Before | After |
|--------|-------|
| Only a per-tenant 2-minute cool-down | Adds IP-based sliding-window: max **5 resends per hour** per IP address |
| No IP tracking | IP checked before token validation (prevents tenant enumeration via timing) |

### `IRateLimitService` contract
```csharp
public interface IRateLimitService
{
    bool IsAllowed(string key, int maxAttempts, int windowMinutes);
}
```

`InMemoryRateLimitService` uses a `ConcurrentDictionary` with a thread-safe per-key lock. Suitable for single-node; replace with Redis-backed for multi-node.

---

## Change 6 — Subdomain Availability Check Endpoint

**Priority:** LOW
**Files changed:**
- `DreamSoft.Application/Features/Registration/CheckSubdomainAvailability/CheckSubdomainAvailabilityQuery.cs` — **Created**
- `DreamSoft.Application/Features/Registration/CheckSubdomainAvailability/CheckSubdomainAvailabilityQueryHandler.cs` — **Created**
- `DreamSoft.Api/Controllers/RegistrationController.cs` — **Modified**

### New endpoint
```
GET /api/registration/check-subdomain?subdomain={value}
```

**Response:**
```json
{
  "subdomain": "acme",
  "isAvailable": true,
  "reason": null
}
```

**Rejection reasons:** `SubdomainLength`, `SubdomainFormat`, `SubdomainReserved`, `SubdomainTaken`

**Reserved subdomains:** `www`, `api`, `admin`, `app`, `mail`, `smtp`, `ftp`, `help`, `support`, `docs`, `status`, `cdn`, `static`, `dashboard`, `portal`, `login`, `auth`, `register`, `billing`, `account`, `accounts`, `dreamsoft`

---

## Change 7 — Terms of Service Acceptance Tracking

**Priority:** LOW
**Files changed:**
- `DreamSoft.Domain/Entities/Tenant.cs` — **Modified**
- `DreamSoft.Infrastructure/Persistence/Configurations/TenantConfiguration.cs` — **Modified**
- `DreamSoft.Application/Features/Registration/RegisterTenant/RegisterTenantCommand.cs` — **Modified**
- `DreamSoft.Application/Features/Registration/RegisterTenant/RegisterTenantCommandHandler.cs` — **Modified**
- Migration: `AddTenantTermsAcceptance`

### What changed
Three new columns on the `tenants` table:

| Column | Type | Description |
|--------|------|-------------|
| `terms_version` | `varchar(50)` | ToS version string, e.g. `"2025-01-01"` |
| `terms_accepted_at` | `timestamptz` | UTC timestamp of acceptance |
| `terms_accepted_ip` | `varchar(50)` | IP address of the registering user |

New domain method:
```csharp
tenant.AcceptTerms(version: "2025-01-01", acceptedAt: DateTime.UtcNow, acceptedIp: ip);
```

Wired in `RegisterTenantCommandHandler` when `request.TermsVersion` is provided (optional — backwards compatible).

---

## Database Migrations Summary

| Migration | Description |
|-----------|-------------|
| `AddRefreshTokensTable` | Drops `refresh_token` + `refresh_token_expiry_time` from `users`; creates `refresh_tokens` table with indexes and FK |
| `AddTenantTermsAcceptance` | Adds `terms_version`, `terms_accepted_at`, `terms_accepted_ip` to `tenants` |

---

## Additional Infrastructure Changes

### `ApplicationDbContextFactory` (design-time)
A new `IDesignTimeDbContextFactory<ApplicationDbContext>` was added to `DreamSoft.Infrastructure/Persistence/` so that `dotnet ef migrations add` can run without starting the full API DI container.

### Test suite
All 81 existing integration tests continue to pass. Updated test helpers:
- `StubRateLimitService` — always allows (rate limiting tested separately)
- `RegistrationTestDbContext` — added `RefreshTokens` DbSet + model config + `Ignore(e => e.RefreshTokens)` on `User`

---

## Security Summary

| Threat | Mitigation |
|--------|-----------|
| XSS stealing refresh tokens from localStorage | Refresh token never in JSON body — HTTP-only cookie only |
| CSRF with cookie-based refresh tokens | `SameSite=Strict` prevents cross-site requests from including the cookie |
| Refresh token reuse after theft | Token rotation — each use revokes old token; stale reuse is detectable |
| OTP resend abuse / tenant enumeration | IP-based sliding window (5/hour) + per-tenant 2-minute cool-down |
| Subdomain squatting of reserved names | Reserved list checked at `CheckSubdomainAvailability` and registration |
| Long-lived sessions without expiry awareness | RememberMe gives explicit 30-day sessions; default sessions are 7 days |
| No audit trail for token revocation | `revoked_at` + `revoked_by_ip` stored permanently on `RefreshToken` row |

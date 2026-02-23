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
    // Short session — no RememberMe
    private const int DefaultRefreshTokenExpiryDays = 7;

    // Extended session — RememberMe selected
    private const int ExtendedRefreshTokenExpiryDays = 30;

    public async Task<LoginResponse> Handle(
        LoginBySubdomainCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Resolve subdomain from the HTTP context (set by TenantResolutionMiddleware)
        var subdomain = currentUserService.Subdomain;
        if (string.IsNullOrWhiteSpace(subdomain))
            throw new UnauthorizedException("Subdomain could not be resolved from the request.");

        // 2. Find the tenant by subdomain — global query (Tenants have no tenant filter)
        var tenant = await context.Tenants
            .Include(t => t.Status)
            .FirstOrDefaultAsync(
                t => t.Subdomain == subdomain.ToLower(),
                cancellationToken)
            ?? throw new UnauthorizedException("Invalid credentials.");

        // 3. Tenant must be active to allow logins
        if (tenant.Status.Code != TenantStatusCodes.Active)
            throw new UnauthorizedException("Tenant account is not active.");

        // 4. Find the user by username within that tenant
        var user = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                u => u.TenantId == tenant.Id &&
                     u.Username == request.Username.Trim().ToLower() &&
                     u.IsActive,
                cancellationToken)
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
        var expiryDays    = request.RememberMe ? ExtendedRefreshTokenExpiryDays : DefaultRefreshTokenExpiryDays;
        var refreshExpiry = dateTime.UtcNow.AddDays(expiryDays);
        var expiresAt     = dateTime.UtcNow.AddMinutes(60);

        // 10. Persist the RefreshToken entity (one row per session — multi-device support)
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

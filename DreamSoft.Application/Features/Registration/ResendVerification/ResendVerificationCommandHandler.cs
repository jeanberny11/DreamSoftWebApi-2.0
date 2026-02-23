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
    // IP-based limits: max 5 resends per hour from the same IP address
    private const int MaxResendPerHour = 5;
    private const int WindowMinutes    = 60;

    public async Task<Unit> Handle(
        ResendVerificationCommand request,
        CancellationToken cancellationToken)
    {
        // 1. IP-based rate limit — checked before token validation to prevent enumeration
        var ip = currentUserService.IpAddress ?? "unknown";
        if (!rateLimitService.IsAllowed($"resend-otp:{ip}", MaxResendPerHour, WindowMinutes))
            throw new RateLimitExceededException("RateLimitExceeded");

        // 2. Validate registration token and extract tenantId
        var tenantId = tokenService.GetTenantIdFromRegistrationToken(
            request.RegistrationToken)
            ?? throw new UnauthorizedException("Unauthorized");

        // 3. Load tenant — must still be PENDING_EMAIL_VERIFICATION
        var tenant = await context.Tenants
            .Include(t => t.Status)
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken)
            ?? throw new NotFoundException("TenantNotFound", tenantId);

        if (tenant.Status.Code != TenantStatusCodes.PendingEmailVerification)
            throw new ConflictException("EmailAlreadyVerified");

        // 4. Per-tenant cool-down — reject if any token was created in the last 2 minutes
        var twoMinutesAgo = DateTime.UtcNow.AddMinutes(-2);
        var recentToken = await context.TenantRegistrationTokens
            .AnyAsync(t => t.TenantId == tenantId
                        && t.CreatedAt >= twoMinutesAgo,
                cancellationToken);
        if (recentToken)
            throw new RateLimitExceededException("RateLimitExceeded");

        // 5. Invalidate all existing unconsumed tokens
        var activeTokens = await context.TenantRegistrationTokens
            .Where(t => t.TenantId == tenantId && !t.IsConsumed)
            .ToListAsync(cancellationToken);
        foreach (var t in activeTokens) t.Consume();

        // 6. Generate new OTP and PBKDF2-hash it
        var plainCode = RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();
        var codeHash = passwordHasher.HashPassword(plainCode);
        var newToken = TenantRegistrationToken.Create(
            tenantId: tenantId,
            codeHash: codeHash,
            expiresAt: DateTime.UtcNow.AddHours(24));

        context.TenantRegistrationTokens.Add(newToken);
        await context.SaveChangesAsync(cancellationToken);

        // 7. Load admin user and send new verification email
        var adminUser = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.TenantId == tenantId, cancellationToken)
            ?? throw new NotFoundException("UserNotFound", tenantId);

        await emailService.SendVerificationCodeAsync(
            adminUser.Email, plainCode, cancellationToken);

        return Unit.Value;
    }
}

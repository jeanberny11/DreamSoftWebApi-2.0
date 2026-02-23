using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DreamSoft.Application.Features.Registration.VerifyEmail;

public class VerifyEmailCommandHandler(
    IApplicationDbContext context,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IEmailService emailService,
    ICurrentUserService currentUserService,
    IConfiguration configuration)
    : IRequestHandler<VerifyEmailCommand, VerifyEmailResponse>
{
    public async Task<VerifyEmailResponse> Handle(
        VerifyEmailCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate registration token and extract tenantId
        var tenantId = tokenService.GetTenantIdFromRegistrationToken(
            request.RegistrationToken)
            ?? throw new UnauthorizedException("Unauthorized");

        // 2. Load tenant with status
        var tenant = await context.Tenants
            .Include(t => t.Status)
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken)
            ?? throw new NotFoundException("TenantNotFound", tenantId);

        // 3. Idempotency guard — if already verified, reject
        if (tenant.Status.Code != TenantStatusCodes.PendingEmailVerification)
            throw new ConflictException("EmailAlreadyVerified");

        // 4. Load latest unconsumed token and apply attempt limit
        var maxAttempts = int.Parse(
            configuration["RateLimit:MaxVerificationAttemptsPerCode"] ?? "5");

        var otpToken = await context.TenantRegistrationTokens
            .Where(t => t.TenantId == tenantId && !t.IsConsumed)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new UnauthorizedException("InvalidOtpCode");

        // 5. Validate token state (not expired, not consumed, under attempt limit)
        if (!otpToken.IsValid(maxAttempts))
            throw new UnauthorizedException("InvalidOtpCode");

        // 6. Verify the submitted code against the stored PBKDF2 hash
        var codeValid = passwordHasher.VerifyPassword(
            request.Code, otpToken.CodeHash);

        if (!codeValid)
        {
            otpToken.IncrementAttempt();
            await context.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException("InvalidOtpCode");
        }

        // 7. Load the PENDING_SUBSCRIPTION status
        var pendingSubStatus = await context.TenantStatuses
            .FirstOrDefaultAsync(
                s => s.Code == TenantStatusCodes.PendingSubscription,
                cancellationToken)
            ?? throw new NotFoundException(
                "NotFound",
                "TenantStatus PENDING_SUBSCRIPTION not found. Run migration.");

        // 8. Load the admin user (first user for this tenant)
        var adminUser = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.TenantId == tenantId, cancellationToken)
            ?? throw new NotFoundException("UserNotFound", tenantId);

        // ── Begin transaction ────────────────────────────────────────────────
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            otpToken.Consume();
            tenant.TransitionStatus(pendingSubStatus.Id);
            tenant.VerifyEmail(DateTime.UtcNow);
            adminUser.VerifyEmail();

            await context.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
        // ── Transaction complete ─────────────────────────────────────────────

        // 9. Send welcome email — strictly after commit
        await emailService.SendWelcomeEmailAsync(
            adminUser.Email,
            adminUser.FirstName,
            tenant.CompanyName,
            tenant.Subdomain,
            cancellationToken);

        // 10. Issue real access + refresh tokens
        var accessToken  = tokenService.GenerateAccessToken(adminUser, tenant);
        var rawToken     = tokenService.GenerateRefreshToken();

        var refreshExpiryDays = int.Parse(
            configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");

        var refreshTokenEntity = RefreshToken.Create(
            userId:      adminUser.Id,
            token:       rawToken,
            expiresAt:   DateTime.UtcNow.AddDays(refreshExpiryDays),
            createdByIp: currentUserService.IpAddress);

        context.RefreshTokens.Add(refreshTokenEntity);
        await context.SaveChangesAsync(cancellationToken);

        return new VerifyEmailResponse(accessToken, rawToken);
    }
}

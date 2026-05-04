using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace DreamSoft.Application.Features.Registration.VerifyEmail;

public class VerifyEmailCommandHandler(
    ITenantRepository tenantRepository,
    ITenantStatusRepository tenantStatusRepository,
    ITenantRegistrationTokenRepository tokenRepository,
    ITenantRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    ICurrentUserService currentUserService,
    IRateLimitService rateLimitService,
    IConfiguration configuration)
    : IRequestHandler<VerifyEmailCommand, VerifyEmailResponse>
{
    // IP-based limits: max 5 verify attempts per 10 minutes from the same IP
    private const int MaxVerifyPerWindow = 5;
    private const int VerifyWindowMinutes = 10;

    public async Task<VerifyEmailResponse> Handle(
        VerifyEmailCommand request,
        CancellationToken cancellationToken)
    {
        // 1. IP-based rate limit — prevents brute-force from a single IP
        var ip = currentUserService.IpAddress ?? "unknown";
        if (!rateLimitService.IsAllowed($"verify-email:{ip}", MaxVerifyPerWindow, VerifyWindowMinutes))
            throw new RateLimitExceededException();

        // 2. Lookup tenant by normalized email — same error as wrong code to prevent enumeration
        var tenant = await tenantRepository.GetByEmailWithStatusAsync(
            request.Email, cancellationToken)
            ?? throw new UnauthorizedException("InvalidOtpCode");

        // 3. Idempotency guard — if already verified, reject
        if (tenant.Status.Code != TenantStatusCodes.PendingEmailVerification)
            throw new ConflictException("EmailAlreadyVerified");

        // 4. Load latest unconsumed token and apply attempt limit
        var maxAttempts = int.Parse(
            configuration["RateLimit:MaxVerificationAttemptsPerCode"] ?? "5");

        var otpToken = await tokenRepository.GetActiveTokenForTenantAsync(tenant.Id, cancellationToken)
            ?? throw new UnauthorizedException("InvalidOtpCode");

        // 5. Validate token state (not expired, not consumed, under attempt limit)
        if (!otpToken.IsValid(maxAttempts))
            throw new UnauthorizedException("InvalidOtpCode");

        // 6. Verify the submitted code against the stored PBKDF2 hash
        if (!passwordHasher.VerifyPassword(request.Code, otpToken.CodeHash))
        {
            otpToken.IncrementAttempt();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException("InvalidOtpCode");
        }

        // 7. Load the PENDING_SUBSCRIPTION status
        var pendingSubStatus = await tenantStatusRepository.GetByCodeAsync(
            TenantStatusCodes.PendingSubscription, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "TenantStatus", "Code = PENDING_SUBSCRIPTION");

        // ── Begin transaction ────────────────────────────────────────────────
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            otpToken.Consume();
            tenant.UpdateStatus(pendingSubStatus.Id);
            tenant.VerifyEmail(DateTime.UtcNow);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
        // ── Transaction complete ─────────────────────────────────────────────

        // 8. Issue tenant-tier access + refresh tokens
        var accessToken = tokenService.GenerateTenantAccessToken(tenant);
        var rawToken    = tokenService.GenerateRefreshToken();

        var refreshExpiryDays = int.Parse(
            configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");

        var refreshTokenEntity = TenantRefreshToken.Create(
            tenantId:    tenant.Id,
            token:       rawToken,
            expiresAt:   DateTime.UtcNow.AddDays(refreshExpiryDays),
            createdByIp: currentUserService.IpAddress);

        await refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new VerifyEmailResponse(accessToken, rawToken);
    }
}

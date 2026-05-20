using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace DreamSoft.Application.Features.Apps.LandingApp.VerifyEmail;

public class VerifyEmailCommandHandler(
    ITenantRepository tenantRepository,
    ITenantStatusRepository tenantStatusRepository,
    ITenantRegistrationTokenRepository tokenRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ICurrentTenantService currentTenantService,
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
        var ip = currentTenantService.IpAddress ?? "unknown";
        if (!rateLimitService.IsAllowed($"verify-email:{ip}", MaxVerifyPerWindow, VerifyWindowMinutes))
            throw new RateLimitExceededException();
        
        // 2. Lookup tenant by ID from the access token
        var tenantId = currentTenantService.TenantId
            ?? throw new UnauthorizedException("Unauthorized");

        // 2. Lookup tenant by normalized email — same error as wrong code to prevent enumeration
        var tenant = await tenantRepository.GetByIdWithStatusAsync(tenantId, cancellationToken)
            ?? throw new UnauthorizedException("InvalidOtpCode");

        // 3. Idempotency guard — if already verified, reject with same error as wrong code to prevent enumeration
        if (tenant.EmailVerified)
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

        return new VerifyEmailResponse(tenant.Id, tenant.Email);
    }
}

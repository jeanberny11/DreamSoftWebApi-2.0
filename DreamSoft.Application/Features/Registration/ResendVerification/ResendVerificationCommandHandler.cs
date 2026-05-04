using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using MediatR;
using System.Security.Cryptography;

namespace DreamSoft.Application.Features.Registration.ResendVerification;

public class ResendVerificationCommandHandler(
    ITenantRepository tenantRepository,
    ITenantRegistrationTokenRepository tokenRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
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
        // 1. IP-based rate limit — checked before DB lookup to prevent enumeration
        var ip = currentUserService.IpAddress ?? "unknown";
        if (!rateLimitService.IsAllowed($"resend-otp:{ip}", MaxResendPerHour, WindowMinutes))
            throw new RateLimitExceededException();

        // 2. Lookup tenant by normalized email — same opaque error to prevent user enumeration
        var tenant = await tenantRepository.GetByEmailWithStatusAsync(
            request.Email, cancellationToken)
            ?? throw new UnauthorizedException("Unauthorized");

        // 3. Tenant must still be PENDING_EMAIL_VERIFICATION
        if (tenant.Status.Code != TenantStatusCodes.PendingEmailVerification)
            throw new ConflictException("EmailAlreadyVerified");

        // 4. Per-tenant cool-down — reject if any token was created in the last 2 minutes
        if (await tokenRepository.HasRecentTokenAsync(tenant.Id, minutesAgo: 2, cancellationToken))
            throw new RateLimitExceededException();

        // 5. Invalidate all existing unconsumed tokens
        await tokenRepository.ConsumeAllForTenantAsync(tenant.Id, cancellationToken);

        // 6. Generate new OTP and PBKDF2-hash it
        var plainCode = RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();
        var codeHash  = passwordHasher.HashPassword(plainCode);
        var newToken  = TenantRegistrationToken.Create(
            tenantId:  tenant.Id,
            codeHash:  codeHash,
            expiresAt: DateTime.UtcNow.AddHours(24));

        await tokenRepository.AddAsync(newToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Send new verification email directly to tenant email
        var emailResult = await emailService.SendVerificationCodeAsync(tenant.Email, plainCode, cancellationToken);
        if (!emailResult.IsSuccess)
            throw new EmailSendException(emailResult.Error ?? "Unknown error");

        return Unit.Value;
    }
}

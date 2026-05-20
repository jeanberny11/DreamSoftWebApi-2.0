using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using MediatR;
using System.Security.Cryptography;

namespace DreamSoft.Application.Features.Apps.LandingApp.SendEmailVerificationCode;

public class SendEmailVerificationCodeCommandHandler(
    ITenantRepository tenantRepository,
    ITenantRegistrationTokenRepository tokenRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IEmailService emailService,
    ICurrentTenantService currentTenantService,
    IRateLimitService rateLimitService)
    : IRequestHandler<SendEmailVerificationCodeCommand, SendEmailVerificationCodeResponse>
{
    // IP-based limits: max 5 resends per hour from the same IP address
    private const int MaxResendPerHour = 5;
    private const int WindowMinutes    = 60;

    public async Task<SendEmailVerificationCodeResponse> Handle(
        SendEmailVerificationCodeCommand request,
        CancellationToken cancellationToken)
    {
        // 1. IP-based rate limit — checked before DB lookup to prevent enumeration
        var ip = currentTenantService.IpAddress ?? "unknown";
        if (!rateLimitService.IsAllowed($"resend-otp:{ip}", MaxResendPerHour, WindowMinutes))
            throw new RateLimitExceededException();

        // 2. Lookup tenant by ID from the access token
        var tenantId = currentTenantService.TenantId
            ?? throw new UnauthorizedException("Unauthorized");

        var tenant = await tenantRepository.GetByIdWithStatusAsync(tenantId, cancellationToken)
            ?? throw new UnauthorizedException("Unauthorized");

        // 3. Tenant must still be PENDING_EMAIL_VERIFICATION
        if (tenant.EmailVerified)
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

        return new SendEmailVerificationCodeResponse(tenant.Email, true, "Verification code sent successfully");
    }
}

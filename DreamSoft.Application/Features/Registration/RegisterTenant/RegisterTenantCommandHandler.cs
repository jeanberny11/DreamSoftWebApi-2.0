using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using MediatR;
using System.Security.Cryptography;

namespace DreamSoft.Application.Features.Registration.RegisterTenant;

public class RegisterTenantCommandHandler(
    ITenantRepository tenantRepository,
    ITenantStatusRepository tenantStatusRepository,
    ILanguageRepository languageRepository,
    ITenantRegistrationTokenRepository tokenRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IEmailService emailService,
    ICurrentUserService currentUserService)
    : IRequestHandler<RegisterTenantCommand, RegisterTenantResponse>
{
    public async Task<RegisterTenantResponse> Handle(
        RegisterTenantCommand request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLower();

        // 1. Tenant email uniqueness (global)
        if (await tenantRepository.EmailExistsAsync(email, cancellationToken))
            throw new ConflictException("EmailAlreadyExists", email);

        // 2. Load PENDING_EMAIL_VERIFICATION status
        var pendingStatus = await tenantStatusRepository.GetByCodeAsync(
            TenantStatusCodes.PendingEmailVerification, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "TenantStatus", "Code = PENDING_EMAIL_VERIFICATION");

        // 3. Load default language (Spanish)
        var defaultLanguage = await languageRepository.GetDefaultAsync(cancellationToken);
        var languageId = defaultLanguage?.Id ?? 0;

        // 4. Hash password
        var passwordHash = passwordHasher.HashPassword(request.Password);

        // Declare outside try so it's accessible after commit
        string plainCode = string.Empty;

        // ── Begin transaction ────────────────────────────────────────────────
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // 5. Create tenant
            var tenant = Tenant.Create(
                firstName:    request.FirstName,
                lastName:     request.LastName,
                companyName:  request.CompanyName,
                email:        email,
                passwordHash: passwordHash,
                statusId:     pendingStatus.Id,
                phone:        "",
                addressLine1: "",
                languageId:   languageId);

            if (request.AcceptTerms && !string.IsNullOrWhiteSpace(request.TermsVersion))
            {
                tenant.AcceptTerms(
                    version:    request.TermsVersion,
                    acceptedAt: DateTime.UtcNow,
                    acceptedIp: currentUserService.IpAddress);
            }

            await tenantRepository.AddAsync(tenant, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken); // materialise tenant.Id

            // 6. Generate 6-digit OTP and PBKDF2-hash it
            plainCode    = RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();
            var codeHash = passwordHasher.HashPassword(plainCode);

            // 7. Create OTP token (24-hour expiry)
            var token = TenantRegistrationToken.Create(
                tenantId:  tenant.Id,
                codeHash:  codeHash,
                expiresAt: DateTime.UtcNow.AddHours(24));

            await tokenRepository.AddAsync(token, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
        // ── Transaction complete ─────────────────────────────────────────────

        // 8. Send verification email AFTER commit
        var emailResult = await emailService.SendVerificationCodeAsync(email, plainCode, cancellationToken);
        if (!emailResult.IsSuccess)
            throw new EmailSendException(emailResult.Error ?? "Unknown error");

        return new RegisterTenantResponse(email, "Verification email sent");
    }
}

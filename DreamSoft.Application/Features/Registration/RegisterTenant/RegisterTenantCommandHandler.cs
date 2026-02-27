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
    IUserRepository userRepository,
    ITenantStatusRepository tenantStatusRepository,
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
        // 1. Subdomain uniqueness (global — no tenant query filter applies to Tenants)
        var subdomainTaken = await tenantRepository.ExistsBySubdomainAsync(
            request.Subdomain, cancellationToken);
        if (subdomainTaken)
            throw new ConflictException("TenantAlreadyExists", request.Subdomain);

        // 2. Admin email uniqueness (global across all tenants)
        var emailTaken = await userRepository.ExistsByEmailGloballyAsync(
            request.AdminEmail, cancellationToken);
        if (emailTaken)
            throw new ConflictException("EmailAlreadyExists", request.AdminEmail);

        // 3. Load PENDING_EMAIL_VERIFICATION status
        var pendingStatus = await tenantStatusRepository.GetByCodeAsync(
            TenantStatusCodes.PendingEmailVerification, cancellationToken)
            ?? throw new NotFoundException(
                "NotFound",
                "TenantStatus PENDING_EMAIL_VERIFICATION not found. Run migration.");

        // Declare outside the try block so they are accessible after commit
        string plainCode  = string.Empty;
        string adminEmail = request.AdminEmail.Trim().ToLower();

        // ── Begin transaction ────────────────────────────────────────────────
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // 4. Create tenant
            var tenant = Tenant.Create(
                companyName:  request.CompanyName,
                subdomain:    request.Subdomain,
                email:        adminEmail,
                currencyId:   request.CurrencyId,
                languageId:   request.LanguageId,
                statusId:     pendingStatus.Id,
                taxId:        request.TaxId,
                phone:        request.Phone);

            tenant.UpdateAddress(
                addressLine1:   request.AddressLine1,
                countryId:      request.CountryId,
                provinceId:     request.ProvinceId,
                municipalityId: request.MunicipalityId);

            if (!string.IsNullOrWhiteSpace(request.TermsVersion))
            {
                tenant.AcceptTerms(
                    version:    request.TermsVersion,
                    acceptedAt: DateTime.UtcNow,
                    acceptedIp: currentUserService.IpAddress);
            }

            await tenantRepository.AddAsync(tenant, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken); // materialise tenant.Id

            // 5. Create admin user (username = email)
            var passwordHash = passwordHasher.HashPassword(request.AdminPassword);
            var user = User.Create(
                tenantId:     tenant.Id,
                username:     adminEmail,
                email:        adminEmail,
                passwordHash: passwordHash,
                firstName:    request.AdminFirstName,
                lastName:     request.AdminLastName,
                languageId:   request.LanguageId,
                isAdmin:      true);

            // 6. Generate 6-digit OTP and PBKDF2-hash it
            plainCode        = GenerateSixDigitCode();
            var codeHash     = passwordHasher.HashPassword(plainCode);

            // 7. Create OTP token (24-hour expiry)
            var token = TenantRegistrationToken.Create(
                tenantId:  tenant.Id,
                codeHash:  codeHash,
                expiresAt: DateTime.UtcNow.AddHours(24));

            await userRepository.AddAsync(user, cancellationToken);
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

        // 8. Send verification email AFTER commit — never for a rolled-back operation
        await emailService.SendVerificationCodeAsync(
            adminEmail, plainCode, cancellationToken);

        return new RegisterTenantResponse(adminEmail, request.Subdomain.ToLower().Trim());
    }

    private static string GenerateSixDigitCode()
        => RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();
}

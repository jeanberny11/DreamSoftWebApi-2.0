using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace DreamSoft.Application.Features.Registration.RegisterTenant;

public class RegisterTenantCommandHandler(
    IApplicationDbContext context,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IEmailService emailService)
    : IRequestHandler<RegisterTenantCommand, RegisterTenantResponse>
{
    public async Task<RegisterTenantResponse> Handle(
        RegisterTenantCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Subdomain uniqueness (global — no tenant query filter applies to Tenants)
        var subdomainTaken = await context.Tenants
            .AnyAsync(t => t.Subdomain == request.Subdomain.ToLower().Trim(),
                cancellationToken);
        if (subdomainTaken)
            throw new ConflictException("TenantAlreadyExists", request.Subdomain);

        // 2. Admin email uniqueness (global across all tenants)
        var emailTaken = await context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == request.AdminEmail.Trim().ToLower(),
                cancellationToken);
        if (emailTaken)
            throw new ConflictException("EmailAlreadyExists", request.AdminEmail);

        // 3. Load PENDING_EMAIL_VERIFICATION status
        var pendingStatus = await context.TenantStatuses
            .FirstOrDefaultAsync(
                s => s.Code == TenantStatusCodes.PendingEmailVerification,
                cancellationToken)
            ?? throw new NotFoundException(
                "NotFound",
                "TenantStatus PENDING_EMAIL_VERIFICATION not found. Run migration.");

        // Declare outside the try block so they are accessible after commit
        string plainCode = string.Empty;
        int tenantId = 0;
        string adminEmail = request.AdminEmail.Trim().ToLower();
        string adminFirstName = request.AdminFirstName.Trim();

        // ── Begin transaction ────────────────────────────────────────────────
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // 4. Create tenant
            var tenant = Tenant.Create(
                companyName: request.CompanyName,
                subdomain: request.Subdomain,
                email: adminEmail,
                currencyId: request.CurrencyId,
                languageId: request.LanguageId,
                statusId: pendingStatus.Id,
                taxId: request.TaxId,
                phone: request.Phone);

            // Set address fields
            tenant.UpdateAddress(
                addressLine1: request.AddressLine1,
                countryId: request.CountryId,
                provinceId: request.ProvinceId,
                municipalityId: request.MunicipalityId);

            context.Tenants.Add(tenant);
            await context.SaveChangesAsync(cancellationToken); // materialise tenant.Id

            tenantId = tenant.Id;

            // 5. Create admin user (username = email)
            var passwordHash = passwordHasher.HashPassword(request.AdminPassword);
            var user = User.Create(
                tenantId: tenantId,
                username: adminEmail,
                email: adminEmail,
                passwordHash: passwordHash,
                firstName: request.AdminFirstName,
                lastName: request.AdminLastName,
                languageId: request.LanguageId);

            // 6. Generate 6-digit OTP and PBKDF2-hash it
            plainCode = GenerateSixDigitCode();
            var codeHash = passwordHasher.HashPassword(plainCode);

            // 7. Create OTP token (24-hour expiry)
            var token = TenantRegistrationToken.Create(
                tenantId: tenantId,
                codeHash: codeHash,
                expiresAt: DateTime.UtcNow.AddHours(24));

            context.Users.Add(user);
            context.TenantRegistrationTokens.Add(token);
            await context.SaveChangesAsync(cancellationToken);

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

        // 9. Return short-lived registration JWT (gates the verify-email step)
        var registrationToken = tokenService.GenerateRegistrationToken(tenantId);
        return new RegisterTenantResponse(registrationToken);
    }

    private static string GenerateSixDigitCode()
        => RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();
}

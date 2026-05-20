using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.Registration.RegisterTenant;

public class RegisterTenantCommandHandler(
    ITenantRepository tenantRepository,
    ITenantStatusRepository tenantStatusRepository,
    ILanguageRepository languageRepository,
    ITenantRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    ICurrentTenantService currentTenantService,
    IDateTime dateTime)
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

        // ── Begin transaction ────────────────────────────────────────────────
        Tenant tenant;
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // 5. Create tenant
            tenant = Tenant.Create(
                firstName: request.FirstName,
                lastName: request.LastName,
                companyName: request.CompanyName,
                email: email,
                passwordHash: passwordHash,
                statusId: pendingStatus.Id,
                phone: "",
                addressLine1: "",
                languageId: languageId);

            if (request.AcceptTerms && !string.IsNullOrWhiteSpace(request.TermsVersion))
            {
                tenant.AcceptTerms(
                    version: request.TermsVersion,
                    acceptedAt: DateTime.UtcNow,
                    acceptedIp: currentTenantService.IpAddress);
            }

            await tenantRepository.AddAsync(tenant, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken); // materialise tenant.Id
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
        // ── Transaction complete ─────────────────────────────────────────────

        // Issue tokens
        var accessToken = tokenService.GenerateTenantAccessToken(tenant);
        var rawToken = tokenService.GenerateRefreshToken();
        var refreshExpiry = dateTime.UtcNow.AddDays(7);
        var expiresAt = dateTime.UtcNow.AddMinutes(60);

        var refreshTokenEntity = TenantRefreshToken.Create(
            tenantId: tenant.Id,
            token: rawToken,
            expiresAt: refreshExpiry,
            createdByIp: currentTenantService.IpAddress,
            deviceInfo: null);

        await refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterTenantResponse(
            AccessToken: accessToken,
            RefreshToken: rawToken,
            ExpiresAt: expiresAt,
            TenantId: tenant.Id,
            Email: email,
            FirstName: tenant.FirstName,
            LastName: tenant.LastName,
            LogoUrl: tenant.LogoUrl,
            TenantStatus: TenantStatusCodes.PendingEmailVerification);
    }
}

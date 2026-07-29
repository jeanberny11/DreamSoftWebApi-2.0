using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Apps.LandingApp.TenantAuth.Dtos;
using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.LandingApp.TenantAuth.LoginTenant;

public class LoginTenantCommandHandler(
    ITenantRepository tenantRepository,
    ITenantRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenantService,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IDateTime dateTime)
    : IRequestHandler<LoginTenantCommand, TenantAuthResponse>
{
    private const int DefaultRefreshTokenExpiryDays  = 7;
    private const int ExtendedRefreshTokenExpiryDays = 30;

    public async Task<TenantAuthResponse> Handle(
        LoginTenantCommand request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        // 1. Load tenant with status — return generic error if not found to prevent enumeration
        var tenant = await tenantRepository.GetByEmailWithStatusAsync(email, cancellationToken)
            ?? throw new UnauthorizedException(ErrorMessageKeys.InvalidCredentials);

        // 2. Always verify password before revealing any account state (prevents timing attacks)
        if (!passwordHasher.VerifyPassword(request.Password, tenant.PasswordHash))
        {
            tenant.RecordFailedLogin();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException(ErrorMessageKeys.InvalidCredentials);
        }

        // 3. Check account lockout — credentials were valid, but account is temporarily blocked
        if (tenant.IsLockedOut())
        {
            var remaining = (int)Math.Ceiling(
                (tenant.LockoutUntil!.Value - dateTime.UtcNow).TotalMinutes);
            throw new ForbiddenException(
                ForbiddenErrorCodes.AccountLocked,
                ErrorMessageKeys.AccountLocked,
                remaining);
        }

        // 4. Check tenant status
        switch (tenant.Status.Code)
        {
            case TenantStatusCodes.Suspended:
                throw new ForbiddenException(
                    ForbiddenErrorCodes.TenantSuspended,
                    ErrorMessageKeys.AccountSuspended);

            case TenantStatusCodes.Cancelled:
                throw new ForbiddenException(
                    ForbiddenErrorCodes.TenantCancelled,
                    ErrorMessageKeys.AccountCancelled);
        }

        // 5. Successful login — reset lockout counter and record last login timestamp
        tenant.ResetFailedLoginAttempts();
        tenant.RecordLogin();

        // 6. Issue tokens
        var accessToken   = tokenService.GenerateTenantAccessToken(tenant);
        var rawToken      = tokenService.GenerateRefreshToken();
        var expiryDays    = request.RememberMe ? ExtendedRefreshTokenExpiryDays : DefaultRefreshTokenExpiryDays;
        var refreshExpiry = dateTime.UtcNow.AddDays(expiryDays);
        var expiresAt     = dateTime.UtcNow.AddMinutes(60);

        // 7. Persist the TenantRefreshToken (one row per session — multi-device support)
        var refreshTokenEntity = TenantRefreshToken.Create(
            tenantId:    tenant.Id,
            token:       rawToken,
            expiresAt:   refreshExpiry,
            createdByIp: currentTenantService.IpAddress,
            deviceInfo:  request.DeviceInfo);

        await refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new TenantAuthResponse(
            AccessToken:        accessToken,
            RefreshToken:       rawToken,
            ExpiresAt:          expiresAt,
            TenantId:           tenant.Id,
            Email:              tenant.Email,
            FirstName:          tenant.FirstName,
            LastName:           tenant.LastName,
            LogoUrl:            tenant.LogoUrl,
            OnboardingCompleted: tenant.OnboardingCompleted,
            EmailVerified:      tenant.EmailVerified,
            TenantStatusCode:         tenant.Status.Code);
    }
}

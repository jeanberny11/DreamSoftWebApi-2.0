using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.ErpApp.Auth.LoginByTenantEmail;

public class LoginByTenantEmailCommandHandler(
    ITenantRepository tenantRepository,
    ITenantSubscriptionRepository tenantSubscriptionRepository,
    IUserRepository userRepository,
    ITenantSubdomainRepository tenantSubdomainRepository,
    IUserRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IDateTime dateTime)
    : IRequestHandler<LoginByTenantEmailCommand, LoginResponse>
{
    private const int DefaultRefreshTokenExpiryDays  = 7;
    private const int ExtendedRefreshTokenExpiryDays = 30;

    public async Task<LoginResponse> Handle(
        LoginByTenantEmailCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Find the tenant by company email — global query (no tenant filter on Tenants)
        var tenant = await tenantRepository.GetByEmailWithStatusAsync(
            request.TenantEmail, cancellationToken)
            ?? throw new UnauthorizedException("Invalid credentials.");

        // 2. Find the user by username within that tenant (cross-solution search for mobile clients)
        var user = await userRepository.GetByUsernameAndTenantAsync(
            tenant.Id, request.Username, cancellationToken)
            ?? throw new UnauthorizedException("Invalid credentials.");

        // 3. Verify password — always check before revealing any account/tenant state
        if (!passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException("Invalid credentials.");
        }

        // 4. Credentials are valid — now check tenant state
        switch (tenant.Status.Code)
        {
            case TenantStatusCodes.PendingEmailVerification:
                throw new ForbiddenException(
                    ForbiddenErrorCodes.TenantPendingEmailVerification,
                    ErrorMessageKeys.EmailVerificationRequired);

            case TenantStatusCodes.PendingSubscription:
                throw new ForbiddenException(
                    ForbiddenErrorCodes.TenantPendingSubscription,
                    ErrorMessageKeys.TenantPendingSubscription);

            case TenantStatusCodes.Suspended:
                throw new ForbiddenException(
                    ForbiddenErrorCodes.TenantSuspended,
                    ErrorMessageKeys.AccountSuspended);

            case TenantStatusCodes.Cancelled:
                throw new ForbiddenException(
                    ForbiddenErrorCodes.TenantCancelled,
                    ErrorMessageKeys.AccountCancelled);
        }

        // 5. Validate subscription status for this tenant + solution
        var subscription = await tenantSubscriptionRepository
            .GetByTenantAndSolutionAsync(tenant.Id, user.SolutionId, cancellationToken)
            ?? throw new ForbiddenException(
                ForbiddenErrorCodes.SubscriptionNotFound,
                ErrorMessageKeys.SubscriptionNotActive);

        switch (subscription.Status.Code)
        {
            case SubscriptionStatusCodes.Trial:
            case SubscriptionStatusCodes.Active:
                break;

            case SubscriptionStatusCodes.PastDue:
                throw new ForbiddenException(
                    ForbiddenErrorCodes.SubscriptionPastDue,
                    ErrorMessageKeys.SubscriptionPastDue);

            case SubscriptionStatusCodes.Suspended:
                throw new ForbiddenException(
                    ForbiddenErrorCodes.SubscriptionSuspended,
                    ErrorMessageKeys.SubscriptionSuspended);

            case SubscriptionStatusCodes.Cancelled:
                throw new ForbiddenException(
                    ForbiddenErrorCodes.SubscriptionCancelled,
                    ErrorMessageKeys.SubscriptionCancelled);

            case SubscriptionStatusCodes.Expired:
                throw new ForbiddenException(
                    ForbiddenErrorCodes.SubscriptionExpired,
                    ErrorMessageKeys.SubscriptionExpired);

            default:
                // Covers PROCESSING_PAYMENT, PAYMENT_FAILED, and any future states
                throw new ForbiddenException(
                    ForbiddenErrorCodes.SubscriptionPaymentFailed,
                    ErrorMessageKeys.SubscriptionPaymentFailed);
        }

        // 6. Check account lockout
        if (user.IsLockedOut())
        {
            var remaining = (int)Math.Ceiling(
                (user.LockoutUntil!.Value - dateTime.UtcNow).TotalMinutes);
            throw new ForbiddenException(
                ForbiddenErrorCodes.AccountLocked,
                ErrorMessageKeys.AccountLocked,
                remaining);
        }

        // 7. Successful login — reset lockout and record LastLoginAt
        user.ResetFailedLoginAttempts();
        user.RecordLogin();

        // 8. Resolve subdomain via TenantSubdomain
        var tenantSubdomain = await tenantSubdomainRepository
            .GetByTenantAndSolutionAsync(tenant.Id, user.SolutionId, cancellationToken);
        var subdomain = tenantSubdomain?.Subdomain ?? string.Empty;

        // 9. Issue tokens
        var accessToken   = tokenService.GenerateAccessToken(user, tenant);
        var rawToken      = tokenService.GenerateRefreshToken();
        var expiryDays    = request.RememberMe ? ExtendedRefreshTokenExpiryDays : DefaultRefreshTokenExpiryDays;
        var refreshExpiry = dateTime.UtcNow.AddDays(expiryDays);
        var expiresAt     = dateTime.UtcNow.AddMinutes(60);

        // 10. Persist the UserRefreshToken entity (one row per session — multi-device support)
        var refreshTokenEntity = UserRefreshToken.Create(
            userId:      user.Id,
            token:       rawToken,
            expiresAt:   refreshExpiry,
            createdByIp: currentUserService.IpAddress,
            deviceInfo:  request.DeviceInfo);

        await refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponse(
            AccessToken:     accessToken,
            RefreshToken:    rawToken,
            ExpiresAt:       expiresAt,
            UserId:          user.Id,
            Username:        user.Username,
            FullName:        user.GetFullName(),
            TenantSubdomain: subdomain);
    }
}

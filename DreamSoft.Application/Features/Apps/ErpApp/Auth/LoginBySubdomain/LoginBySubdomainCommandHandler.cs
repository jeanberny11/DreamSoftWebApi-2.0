using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Domain.Constants;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.ErpApp.Auth.LoginBySubdomain;

public class LoginBySubdomainCommandHandler(
    ITenantRepository tenantRepository,
    ITenantSubdomainRepository tenantSubdomainRepository,
    ITenantSubscriptionRepository tenantSubscriptionRepository,
    IUserRepository userRepository,
    IUserRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IDateTime dateTime)
    : IRequestHandler<LoginBySubdomainCommand, LoginResponse>
{
    private const int DefaultRefreshTokenExpiryDays  = 7;
    private const int ExtendedRefreshTokenExpiryDays = 30;

    public async Task<LoginResponse> Handle(
        LoginBySubdomainCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Resolve subdomain from the HTTP context (set by TenantResolutionMiddleware)
        var subdomain = currentUserService.Subdomain;
        if (string.IsNullOrWhiteSpace(subdomain))
            throw new UnauthorizedException("Subdomain could not be resolved from the request.");

        // 2. Resolve TenantSubdomain to get TenantId + SolutionId
        var tenantSubdomain = await tenantSubdomainRepository.GetBySubdomainAsync(subdomain, cancellationToken)
            ?? throw new UnauthorizedException("Invalid credentials.");

        // 2b. Load tenant with status
        var tenant = await tenantRepository.GetByIdWithStatusAsync(tenantSubdomain.TenantId, cancellationToken)
            ?? throw new UnauthorizedException("Invalid credentials.");

        // 3. Find the user by username within that tenant + solution
        var user = await userRepository.GetByUsernameAsync(
            tenantSubdomain.TenantId, tenantSubdomain.SolutionId, request.Username, cancellationToken)
            ?? throw new UnauthorizedException("Invalid credentials.");

        // 4. Verify password — always check before revealing any account/tenant state
        if (!passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException("Invalid credentials.");
        }

        // 5. Credentials are valid — now check tenant state
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

        // 6. Validate subscription status for this tenant + solution
        var subscription = await tenantSubscriptionRepository
            .GetByTenantAndSolutionAsync(tenantSubdomain.TenantId, tenantSubdomain.SolutionId, cancellationToken)
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

        // 7. Check account lockout
        if (user.IsLockedOut())
        {
            var remaining = (int)Math.Ceiling(
                (user.LockoutUntil!.Value - dateTime.UtcNow).TotalMinutes);
            throw new ForbiddenException(
                ForbiddenErrorCodes.AccountLocked,
                ErrorMessageKeys.AccountLocked,
                remaining);
        }

        // 8. Successful login — reset lockout and record LastLoginAt
        user.ResetFailedLoginAttempts();
        user.RecordLogin();

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

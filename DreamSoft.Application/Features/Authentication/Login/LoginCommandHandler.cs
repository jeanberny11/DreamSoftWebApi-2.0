using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Authentication.Login;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;


namespace DreamSoft.Application.Features.Authentication.Commands.Login;

/// <summary>
/// Handler for user login with subdomain-based tenant identification
/// Flow: Extract subdomain from request → Find Tenant → Find User by (tenant_id + username)
/// </summary>
public class LoginCommandHandler(
    IApplicationDbContext context,
    IJwtService jwtService,
    IPasswordHasher passwordHasher,
    ICurrentUserService currentUserService,
    ILogger<LoginCommandHandler> logger) : IRequestHandler<LoginRequest, LoginResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IJwtService _jwtService = jwtService;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly ILogger<LoginCommandHandler> _logger = logger;

    public async Task<LoginResponse> Handle(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var username = request.Username.ToLowerInvariant();
        var ipAddress = _currentUserService.IpAddress ?? "Unknown";

        // STEP 1: Extract subdomain from HTTP request
        var subdomain = _currentUserService.Subdomain;

        if (string.IsNullOrEmpty(subdomain))
        {
            _logger.LogWarning(
                "Login failed - no subdomain in request. IP: {IpAddress}",
                ipAddress);

            throw new UnauthorizedException("Unauthorized");
        }

        _logger.LogInformation(
            "Login attempt for subdomain: {Subdomain}, username: {Username} from IP: {IpAddress}",
            subdomain, username, ipAddress);

        // Use resource key for invalid credentials
        const string invalidCredentialsMessage = "InvalidCredentials";

        // STEP 2: Find tenant by subdomain
        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Subdomain == subdomain, cancellationToken);

        if (tenant == null)
        {
            _logger.LogWarning(
                "Login failed - tenant not found for subdomain: {Subdomain}, IP: {IpAddress}",
                subdomain, ipAddress);

            throw new UnauthorizedException(invalidCredentialsMessage);
        }

        // Check if tenant is active
        if (!tenant.IsActive)
        {
            _logger.LogWarning(
                "Login failed - tenant account is inactive. TenantId: {Id}, Subdomain: {Subdomain}, IP: {IpAddress}",
                tenant.Id, subdomain, ipAddress);

            throw new UnauthorizedException("Unauthorized");
        }

        // STEP 3: Find user by tenant_id + username
        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.TenantId == tenant.Id &&
                u.Username == username,
                cancellationToken);

        if (user == null)
        {
            _logger.LogWarning(
                "Login failed - user not found. TenantId: {TenantId}, Username: {Username}, IP: {IpAddress}",
                tenant.Id, username, ipAddress);

            throw new UnauthorizedException(invalidCredentialsMessage);
        }

        // Check if user is active
        if (!user.IsActive)
        {
            _logger.LogWarning(
                "Login failed - user account is inactive. UserId: {Id}, IP: {IpAddress}",
                user.Id, ipAddress);

            throw new UnauthorizedException("Unauthorized");
        }

        // STEP 4: Verify password
        var passwordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);

        if (!passwordValid)
        {
            _logger.LogWarning(
                "Login failed - invalid password. UserId: {UserId}, IP: {IpAddress}",
                user.Id, ipAddress);

            throw new UnauthorizedException(invalidCredentialsMessage);
        }

        // STEP 5: Update last login timestamp
        user.RecordSuccessfulLogin();
        await _context.SaveChangesAsync(cancellationToken);

        // STEP 6: Generate access token (with tenant email and username)
        var accessToken = _jwtService.GenerateAccessToken(
            userId: user.Id,
            tenantId: user.TenantId,
            email: tenant.Email,        // ← Tenant email (for reference only)
            username: user.Username,     // ← User username
            isAdmin: user.IsAdmin);

        // STEP 7: Generate refresh token
        var refreshTokenString = _jwtService.GenerateRefreshToken();

        // STEP 8: Create and save refresh token entity
        var refreshToken = Domain.Entities.RefreshToken.Create(
            tenantId: user.TenantId,
            userId: user.Id,
            token: refreshTokenString,
            createdByIp: ipAddress,
            expiresAt: DateTime.UtcNow.AddDays(request.RememberMe ? 30 : 7)); // 30 days if "remember me", else 7 days

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Login successful. UserId: {UserId}, TenantId: {TenantId}, Subdomain: {Subdomain}, Username: {Username}, IP: {IpAddress}",
            user.Id, user.TenantId, subdomain, user.Username, ipAddress);

        // STEP 9: Return success response
        return new LoginResponse
        {
            Success = true,
            Message = "Login successful!",
            UserId = user.Id,
            Email = tenant.Email,           // ← Tenant email (for display/reference)
            Username = user.Username,       // ← User username
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsAdmin = user.IsAdmin,
            TenantId = user.TenantId,
            TenantNumber = tenant.TenantNumber,
            CompanyName = tenant.CompanyName,
            Subdomain = tenant.Subdomain,
            AccessToken = accessToken,
            AccessTokenExpiresInSeconds = 3600, // 1 hour
            RefreshToken = refreshTokenString   // ← Refresh token for HTTP-only cookie
        };
    }
}

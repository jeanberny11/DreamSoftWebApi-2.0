using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Authentication.Login;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Application.Features.Authentication.Commands.Login;

/// <summary>
/// Handler for user login
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

        _logger.LogInformation(
            "Login attempt for username: {Username} from IP: {IpAddress}",
            username, ipAddress);

        // Generic error message to prevent user enumeration
        const string invalidCredentialsMessage = "Invalid email or password.";

        // 1. Find user by email (include tenant data)
        var user = await _context.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning(
                "Login failed - user not found. Username: {Username}, IP: {IpAddress}",
                username, ipAddress);

            throw new UnauthorizedException(invalidCredentialsMessage);
        }

        // 2. Check if user is active
        if (!user.IsActive)
        {
            _logger.LogWarning(
                "Login failed - user account is inactive. UserId: {UserId}, IP: {IpAddress}",
                user.Id, ipAddress);

            throw new UnauthorizedException("Your account has been deactivated. Please contact support.");
        }

        // 3. Check if tenant is active
        if (!user.Tenant.IsActive)
        {
            _logger.LogWarning(
                "Login failed - tenant account is inactive. TenantId: {TenantId}, IP: {IpAddress}",
                user.TenantId, ipAddress);

            throw new UnauthorizedException("Your organization's account has been deactivated. Please contact support.");
        }

        // 4. Verify password
        var passwordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);

        if (!passwordValid)
        {
            _logger.LogWarning(
                "Login failed - invalid password. UserId: {UserId}, IP: {IpAddress}",
                user.Id, ipAddress);

            throw new UnauthorizedException(invalidCredentialsMessage);
        }

        // 5. Update last login timestamp
        user.RecordSuccessfulLogin();
        await _context.SaveChangesAsync(cancellationToken);

        // 6. Generate access token
        var accessToken = _jwtService.GenerateAccessToken(
            userId: user.Id,
            tenantId: user.TenantId,
            email: user.Tenant.Email,
            username: user.Username,
            isAdmin: user.IsAdmin);

        // 7. Generate refresh token
        var refreshTokenString = _jwtService.GenerateRefreshToken();

        // 8. Create and save refresh token entity
        var refreshToken = Domain.Entities.RefreshToken.Create(
            tenantId: user.TenantId,
            userId: user.Id,
            token: refreshTokenString,
            createdByIp: ipAddress,
            expiresAt: DateTime.UtcNow.AddDays(request.RememberMe ? 30 : 1)); // 30 days if "remember me", else 1 day

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Login successful. UserId: {UserId}, TenantId: {TenantId}, IP: {IpAddress}",
            user.Id, user.TenantId, ipAddress);

        // 9. Return success response
        return new LoginResponse
        {
            Success = true,
            Message = "Login successful!",
            UserId = user.Id,
            Email = user.Tenant.Email,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsAdmin = user.IsAdmin,
            TenantId = user.TenantId,
            TenantNumber = user.Tenant.TenantNumber,
            CompanyName = user.Tenant.CompanyName,
            Subdomain = user.Tenant.Subdomain,
            AccessToken = accessToken,
            AccessTokenExpiresInSeconds = 3600 // 1 hour
        };
    }
}
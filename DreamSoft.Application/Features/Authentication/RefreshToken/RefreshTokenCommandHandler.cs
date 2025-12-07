using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Authentication.RefreshToken;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DreamSoft.Application.Features.Authentication.Commands.RefreshToken;

/// <summary>
/// Handler for refreshing access token using refresh token
/// </summary>
public class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenRequest, RefreshTokenResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        IJwtService jwtService,
        ICurrentUserService currentUserService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public  Task<RefreshTokenResponse> Handle(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        // Note: The refresh token is expected to be in an HTTP-only cookie
        // The API controller will extract it and pass it here
        // For now, we'll throw an exception as the token comes from cookies
        throw new UnauthorizedException("Refresh token must be provided via HTTP-only cookie.");

        // This handler will be called from the controller with the token extracted from cookies
        // See Phase 8 for controller implementation
    }

    /// <summary>
    /// Internal method to handle refresh token logic (called by controller)
    /// </summary>
    public async Task<RefreshTokenResponse> HandleRefreshToken(
        string refreshTokenString,
        CancellationToken cancellationToken)
    {
        var ipAddress = _currentUserService.IpAddress ?? "Unknown";

        _logger.LogInformation(
            "Refresh token request from IP: {IpAddress}",
            ipAddress);

        // 1. Validate and get refresh token from database
        var refreshToken = await _jwtService.ValidateRefreshTokenAsync(refreshTokenString);

        // ValidateRefreshTokenAsync throws UnauthorizedException if invalid
        // If we reach here, the token is valid

        // 2. Get user with tenant data
        var user = await _context.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Id == refreshToken.UserId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning(
                "Refresh token validation failed - user not found. UserId: {UserId}",
                refreshToken.UserId);

            throw new UnauthorizedException("Invalid refresh token.");
        }

        // 3. Check if user is active
        if (!user.IsActive)
        {
            _logger.LogWarning(
                "Refresh token validation failed - user inactive. UserId: {UserId}",
                user.Id);

            throw new UnauthorizedException("Your account has been deactivated.");
        }

        // 4. Check if tenant is active
        if (!user.Tenant.IsActive)
        {
            _logger.LogWarning(
                "Refresh token validation failed - tenant inactive. TenantId: {TenantId}",
                user.TenantId);

            throw new UnauthorizedException("Your organization's account has been deactivated.");
        }

        // 5. Revoke the old refresh token
        refreshToken.Revoke(ipAddress);
        await _context.SaveChangesAsync(cancellationToken);

        // 6. Generate new access token
        var accessToken = _jwtService.GenerateAccessToken(
            userId: user.Id,
            tenantId: user.TenantId,
            email: "",
            username: user.Username,
            isAdmin: user.IsAdmin);

        // 7. Generate new refresh token
        var newRefreshTokenString = _jwtService.GenerateRefreshToken();
        var newRefreshToken = Domain.Entities.RefreshToken.Create(
            userId: user.Id,
            token: newRefreshTokenString,
            createdByIp: ipAddress,
            expiresAt: 7); // Default 7 days

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Access token refreshed successfully. UserId: {UserId}, IP: {IpAddress}",
            user.UserId, ipAddress);

        // 8. Return success response
        return new RefreshTokenResponse
        {
            Success = true,
            Message = "Token refreshed successfully.",
            AccessToken = accessToken,
            AccessTokenExpiresInSeconds = 3600 // 1 hour
        };
    }
}
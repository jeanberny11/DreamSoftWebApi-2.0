using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Authentication.RefreshToken;
using DreamSoft.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DreamSoft.Application.Features.Authentication.Commands.RefreshToken;

/// <summary>
/// Handler for refreshing access token using refresh token
/// Implements token rotation for security (old token is revoked, new token is issued)
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

    public async Task<RefreshTokenResponse> Handle(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var ipAddress = _currentUserService.IpAddress ?? "Unknown";

        _logger.LogInformation(
            "Refresh token request from IP: {IpAddress}",
            ipAddress);

        // STEP 1: Validate and get refresh token from database
        var refreshTokenEntity = await _jwtService.ValidateRefreshTokenAsync(
            request.RefreshToken,
            cancellationToken);

        // ValidateRefreshTokenAsync throws UnauthorizedException if invalid
        // If we reach here, the token is valid

        // STEP 2: Get user with tenant data
        var user = await _context.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Id == refreshTokenEntity.UserId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning(
                "Refresh token validation failed - user not found. UserId: {UserId}",
                refreshTokenEntity.UserId);

            throw new UnauthorizedException("Unauthorized");
        }

        // STEP 3: Check if user is active
        if (!user.IsActive)
        {
            _logger.LogWarning(
                "Refresh token validation failed - user inactive. UserId: {UserId}",
                user.Id);

            throw new UnauthorizedException("Unauthorized");
        }

        // STEP 4: Check if tenant is active
        if (!user.Tenant.IsActive)
        {
            _logger.LogWarning(
                "Refresh token validation failed - tenant inactive. TenantId: {TenantId}",
                user.TenantId);

            throw new UnauthorizedException("Unauthorized");
        }

        // STEP 5: Revoke the old refresh token (one-time use for security)
        refreshTokenEntity.Revoke(ipAddress);
        await _context.SaveChangesAsync(cancellationToken);

        // STEP 6: Generate new access token
        var accessToken = _jwtService.GenerateAccessToken(
            userId: user.Id,
            tenantId: user.TenantId,
            email: user.Tenant.Email,    // ← Tenant email
            username: user.Username,
            isAdmin: user.IsAdmin);

        // STEP 7: Generate new refresh token (token rotation for security)
        var newRefreshTokenString = _jwtService.GenerateRefreshToken();
        var newRefreshToken = Domain.Entities.RefreshToken.Create(
            tenantId: user.TenantId,
            userId: user.Id,
            token: newRefreshTokenString,
            createdByIp: ipAddress,
            expiresAt: DateTime.UtcNow.AddDays(7)); // 7 days default

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Access token refreshed successfully. UserId: {UserId}, TenantId: {TenantId}, IP: {IpAddress}",
            user.Id, user.TenantId, ipAddress);

        // STEP 8: Return success response
        // Note: New refresh token will be set as HTTP-only cookie by controller
        return new RefreshTokenResponse
        {
            Success = true,
            Message = "Token refreshed successfully.",
            AccessToken = accessToken,
            AccessTokenExpiresInSeconds = 3600, // 1 hour
            RefreshToken = newRefreshTokenString // Controller will set this as HTTP-only cookie
        };
    }
}

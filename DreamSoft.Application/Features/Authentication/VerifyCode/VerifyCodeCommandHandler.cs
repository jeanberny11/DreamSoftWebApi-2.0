using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Authentication.VerifyCode;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DreamSoft.Application.Features.Authentication.Commands.VerifyCode;

/// <summary>
/// Handler for verifying email verification code
/// </summary>
public class VerifyCodeCommandHandler(
    IRedisService redisService,
    IJwtService jwtService,
    ILogger<VerifyCodeCommandHandler> logger)
        : IRequestHandler<VerifyCodeRequest, VerifyCodeResponse>
{
    private readonly IRedisService _redisService = redisService;
    private readonly IJwtService _jwtService = jwtService;
    private readonly ILogger<VerifyCodeCommandHandler> _logger = logger;

    public async Task<VerifyCodeResponse> Handle(
        VerifyCodeRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.ToLowerInvariant();

        _logger.LogInformation(
            "Verification code validation requested for email: {Email}",
            email);

        // Check verification attempts limit (max 5 attempts per code)
        var canVerify = await _redisService.CheckVerificationAttemptsAsync(email, cancellationToken);
        if (!canVerify)
        {
            _logger.LogWarning(
                "Maximum verification attempts exceeded for email: {Email}",
                email);

            throw new RateLimitExceededException(
                "Maximum verification attempts exceeded. Please request a new code.",
                TimeSpan.FromMinutes(5));
        }

        // Get verification data from Redis
        var verificationData = await _redisService.GetEmailVerificationDataAsync(email, cancellationToken);

        if (verificationData == null)
        {
            _logger.LogWarning(
                "No verification code found for email: {Email}",
                email);

            throw new ValidationException("email","Invalid or expired verification code.");
        }

        // Verify the code matches
        if (verificationData.Code != request.Code)
        {
            // Increment failed attempts
            await _redisService.IncrementVerificationAttemptsAsync(email);

            _logger.LogWarning(
                "Invalid verification code provided for email: {Email}. Attempts: {Attempts}",
                email, verificationData.Attempts + 1);

            throw new ValidationException("email","Invalid verification code.");
        }

        // Code is valid - delete from Redis
        await _redisService.DeleteEmailVerificationCodeAsync(email);

        // Generate session token (valid for 30 minutes)
        var sessionToken = _jwtService.GenerateSessionToken(email);

        _logger.LogInformation(
            "Email verified successfully for: {Email}. Session token generated.",
            email);

        return new VerifyCodeResponse
        {
            Success = true,
            Message = "Email verified successfully. You can now complete your registration.",
            SessionToken = sessionToken,
            ExpiresInSeconds = 1800 // 30 minutes
        };
    }
}
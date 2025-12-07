using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Common.Interfaces;
using DreamSoft.Application.Features.Authentication.Requests;
using DreamSoft.Application.Features.Authentication.SendVerificationCode;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DreamSoft.Application.Features.Authentication.Commands.SendVerificationCode;

/// <summary>
/// Handler for sending email verification code
/// </summary>
public class SendVerificationCodeCommandHandler
    : IRequestHandler<SendVerificationCodeRequest, SendVerificationCodeResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IRedisService _redisService;
    private readonly IEmailService _emailService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SendVerificationCodeCommandHandler> _logger;

    public SendVerificationCodeCommandHandler(
        IApplicationDbContext context,
        IRedisService redisService,
        IEmailService emailService,
        ICurrentUserService currentUserService,
        ILogger<SendVerificationCodeCommandHandler> logger)
    {
        _context = context;
        _redisService = redisService;
        _emailService = emailService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<SendVerificationCodeResponse> Handle(
        SendVerificationCodeRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.ToLowerInvariant();
        var ipAddress = _currentUserService.IpAddress ?? "Unknown";

        _logger.LogInformation(
            "Verification code requested for email: {Email} from IP: {IpAddress}",
            email, ipAddress);

        // Check rate limit for sending codes (max 3 per hour per IP)
        var canSend = await _redisService.CheckEmailSendRateLimitAsync(ipAddress);
        if (!canSend)
        {
            _logger.LogWarning(
                "Rate limit exceeded for IP: {IpAddress} attempting to send code to {Email}",
                ipAddress, email);

            throw new RateLimitExceededException(
                "Too many verification code requests. Please try again later.",
                TimeSpan.FromHours(1));
        }

        // Check if email already exists in database
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == email, cancellationToken);

        if (emailExists)
        {
            _logger.LogWarning(
                "Attempted to send verification code to existing email: {Email}",
                email);

            throw new ConflictException($"An account with email '{email}' already exists.");
        }

        // Generate 6-digit verification code
        var code = GenerateVerificationCode();

        // Store code in Redis (5 minutes expiration)
        await _redisService.SetEmailVerificationCodeAsync(email, code);

        // Send verification email
        var emailSent = await _emailService.SendVerificationCodeAsync(email, code);

        if (!emailSent)
        {
            _logger.LogError(
                "Failed to send verification code email to: {Email}",
                email);

            throw new ApplicationException("Failed to send verification email. Please try again.");
        }

        _logger.LogInformation(
            "Verification code sent successfully to: {Email}",
            email);

        return new SendVerificationCodeResponse
        {
            Success = true,
            Message = "Verification code sent to your email. Please check your inbox.",
            ExpiresInSeconds = 300 // 5 minutes
        };
    }

    /// <summary>
    /// Generate a random 6-digit verification code
    /// </summary>
    private static string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}
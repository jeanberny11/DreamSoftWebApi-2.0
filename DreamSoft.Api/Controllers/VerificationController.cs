using DreamSoft.Application.Features.Authentication.Requests;
using DreamSoft.Application.Features.Authentication.SendVerificationCode;
using DreamSoft.Application.Features.Authentication.VerifyCode;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers;

/// <summary>
/// Verification controller - handles email verification and OTP code management
/// </summary>
[Route("api/verification")]
public class VerificationController : ApiControllerBase
{
    private readonly ILogger<VerificationController> _logger;

    public VerificationController(ILogger<VerificationController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Send verification code to email
    /// Rate limit: 3 requests per hour per IP
    /// </summary>
    /// <param name="request">Email to send verification code</param>
    /// <returns>Success message with expiration time</returns>
    [HttpPost("send-code")]
    [ProducesResponseType(typeof(SendVerificationCodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SendVerificationCode([FromBody] SendVerificationCodeRequest request)
    {
        var response = await Mediator.Send(request);

        _logger.LogInformation("Verification code sent to {Email}", request.Email);

        return Ok(response);
    }

    /// <summary>
    /// Verify the OTP code and get session token
    /// Max 5 attempts per code
    /// </summary>
    /// <param name="request">Email and verification code</param>
    /// <returns>Session token (valid for 30 minutes)</returns>
    [HttpPost("verify-code")]
    [ProducesResponseType(typeof(VerifyCodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
    {
        var response = await Mediator.Send(request);

        _logger.LogInformation("Verification code verified for {Email}", request.Email);

        return Ok(response);
    }
}

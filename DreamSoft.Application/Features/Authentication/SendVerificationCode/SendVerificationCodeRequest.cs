using DreamSoft.Application.Features.Authentication.SendVerificationCode;
using MediatR;

namespace DreamSoft.Application.Features.Authentication.Requests;

/// <summary>
/// Request to send verification code to email
/// </summary>
public class SendVerificationCodeRequest:IRequest<SendVerificationCodeResponse>
{
    public string Email { get; set; } = null!;
}
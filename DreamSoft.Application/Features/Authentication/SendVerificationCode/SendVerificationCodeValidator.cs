using DreamSoft.Application.Common.Validators;
using DreamSoft.Application.Features.Authentication.Requests;
using FluentValidation;

namespace DreamSoft.Application.Features.Authentication.Validators;

/// <summary>
/// Validator for SendVerificationCodeRequest
/// </summary>
public class SendVerificationCodeValidator : AbstractValidator<SendVerificationCodeRequest>
{
    public SendVerificationCodeValidator()
    {
        RuleFor(x => x.Email)
            .ValidEmail(); // ✅ Using custom validator
    }
}
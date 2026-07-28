using FluentValidation;

namespace DreamSoft.Application.Features.Apps.LandingApp.Payment.GetPaymentsByDateRange;

public class GetPaymentsByDateRangeQueryValidator : AbstractValidator<GetPaymentsByDateRangeQuery>
{
    public GetPaymentsByDateRangeQueryValidator()
    {
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("EndDate must be on or after StartDate");
    }
}

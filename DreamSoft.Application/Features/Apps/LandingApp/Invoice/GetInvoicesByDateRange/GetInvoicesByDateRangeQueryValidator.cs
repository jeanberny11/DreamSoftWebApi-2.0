using FluentValidation;

namespace DreamSoft.Application.Features.Apps.LandingApp.Invoice.GetInvoicesByDateRange;

public class GetInvoicesByDateRangeQueryValidator : AbstractValidator<GetInvoicesByDateRangeQuery>
{
    public GetInvoicesByDateRangeQueryValidator()
    {
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("EndDate must be on or after StartDate");
    }
}

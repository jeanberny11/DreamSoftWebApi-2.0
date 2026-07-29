using DreamSoft.Domain.Entities;

namespace DreamSoft.Application.Features.Apps.LandingApp.Payment.Dtos;

public static class PaymentHistoryDtoMapper
{
    public static PaymentHistoryDto ToDto(SubscriptionPayment payment, string language) => new(
        Id:                    payment.Id,
        SubscriptionInvoiceId: payment.SubscriptionInvoiceId,
        Amount:                payment.Amount,
        Currency:              payment.Currency,
        Status:                payment.Status,
        PaymentDate:           payment.PaymentDate,
        StripePaymentId:       payment.StripePaymentId,
        FailureReason:         payment.FailureReason,
        SolutionId:            payment.Invoice.TenantSubscription.SolutionId,
        SolutionName:          payment.Invoice.TenantSubscription.Solution.GetTranslatedName(language),
        SolutionIcon:          payment.Invoice.TenantSubscription.Solution.Icon,
        PlanName:              payment.Invoice.TenantSubscription.SubscriptionPlan.GetTranslatedName(language),
        InvoiceNumber:         payment.Invoice.InvoiceNumber
    );
}

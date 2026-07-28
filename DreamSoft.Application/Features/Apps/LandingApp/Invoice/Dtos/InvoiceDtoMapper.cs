using DreamSoft.Domain.Entities;

namespace DreamSoft.Application.Features.Apps.LandingApp.Invoice.Dtos;

public static class InvoiceDtoMapper
{
    public static InvoiceDto ToDto(SubscriptionInvoice invoice, string language) => new(
        Id:                   invoice.Id,
        TenantSubscriptionId: invoice.TenantSubscriptionId,
        SolutionId:           invoice.TenantSubscription.SolutionId,
        SolutionName:         invoice.TenantSubscription.Solution.GetTranslatedName(language),
        SolutionIcon:         invoice.TenantSubscription.Solution.Icon,
        PlanName:             invoice.TenantSubscription.SubscriptionPlan.GetTranslatedName(language),
        Amount:               invoice.Amount,
        Currency:             invoice.Currency,
        Status:               invoice.Status,
        DueDate:              invoice.DueDate,
        PaidAt:               invoice.PaidAt,
        InvoiceNumber:        invoice.InvoiceNumber,
        BillingReason:        invoice.BillingReason,
        HostedInvoiceUrl:     invoice.HostedInvoiceUrl,
        InvoicePdfUrl:        invoice.InvoicePdfUrl,
        Payments: invoice.Payments
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => new PaymentAttemptDto(
                Id:              p.Id,
                Amount:          p.Amount,
                Currency:        p.Currency,
                Status:          p.Status,
                PaymentDate:     p.PaymentDate,
                StripePaymentId: p.StripePaymentId,
                FailureReason:   p.FailureReason))
            .ToList()
    );
}

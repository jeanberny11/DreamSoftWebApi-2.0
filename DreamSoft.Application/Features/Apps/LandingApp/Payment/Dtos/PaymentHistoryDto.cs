namespace DreamSoft.Application.Features.Apps.LandingApp.Payment.Dtos;

public record PaymentHistoryDto(
    int Id,
    int SubscriptionInvoiceId,
    decimal Amount,
    string Currency,
    string Status,
    DateTime PaymentDate,
    string? StripePaymentId,
    string? FailureReason,
    int SolutionId,
    string SolutionName,
    string SolutionIcon,
    string PlanName,
    string? InvoiceNumber
);

namespace DreamSoft.Application.Features.Apps.LandingApp.Invoice.Dtos;

public record InvoiceDto(
    int Id,
    int TenantSubscriptionId,
    int SolutionId,
    string SolutionName,
    string SolutionIcon,
    string PlanName,
    decimal Amount,
    string Currency,
    string Status,
    DateTime DueDate,
    DateTime? PaidAt,
    string? InvoiceNumber,
    string? BillingReason,
    string? HostedInvoiceUrl,
    string? InvoicePdfUrl,
    IReadOnlyList<PaymentAttemptDto> Payments
);

public record PaymentAttemptDto(
    int Id,
    decimal Amount,
    string Currency,
    string Status,
    DateTime PaymentDate,
    string? StripePaymentId,
    string? FailureReason
);

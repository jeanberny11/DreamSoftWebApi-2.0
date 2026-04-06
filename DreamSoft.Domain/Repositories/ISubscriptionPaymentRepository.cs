using DreamSoft.Domain.Entities;

namespace DreamSoft.Domain.Repositories;

public interface ISubscriptionPaymentRepository : IRepository<SubscriptionPayment>
{
    Task<IReadOnlyList<SubscriptionPayment>> GetByInvoiceIdAsync(int invoiceId, CancellationToken cancellationToken = default);
}

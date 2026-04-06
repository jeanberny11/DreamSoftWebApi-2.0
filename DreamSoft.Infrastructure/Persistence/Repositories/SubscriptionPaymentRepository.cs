using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class SubscriptionPaymentRepository(ApplicationDbContext context)
    : Repository<SubscriptionPayment>(context), ISubscriptionPaymentRepository
{
    public async Task<IReadOnlyList<SubscriptionPayment>> GetByInvoiceIdAsync(int invoiceId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(p => p.SubscriptionInvoiceId == invoiceId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
}

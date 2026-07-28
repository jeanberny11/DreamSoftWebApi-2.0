using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class SubscriptionPaymentRepository(ApplicationDbContext context)
    : Repository<SubscriptionPayment>(context), ISubscriptionPaymentRepository
{
    /// <summary>Shared include chain for display-ready payment queries (Invoice → Solution, translated Plan).</summary>
    private IQueryable<SubscriptionPayment> WithDetails()
        => _dbSet
            .Include(p => p.Invoice).ThenInclude(i => i.TenantSubscription).ThenInclude(ts => ts.Solution)
            .Include(p => p.Invoice).ThenInclude(i => i.TenantSubscription).ThenInclude(ts => ts.SubscriptionPlan).ThenInclude(sp => sp.Translations);

    public async Task<bool> ExistsByStripePaymentIdAndStatusAsync(
        string stripePaymentId,
        string status,
        CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(
            p => p.StripePaymentId == stripePaymentId && p.Status == status,
            cancellationToken);

    public async Task<IReadOnlyList<SubscriptionPayment>> GetByTenantIdWithDetailsAsync(
        int tenantId, int? solutionId = null, CancellationToken cancellationToken = default)
    {
        if (solutionId.HasValue)
            return await GetByTenantAndSolutionIdAsync(tenantId, solutionId.Value, cancellationToken);

        return await WithDetails()
            .Where(p => p.TenantId == tenantId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SubscriptionPayment>> GetByTenantAndSolutionIdAsync(
        int tenantId, int solutionId, CancellationToken cancellationToken = default)
        => await WithDetails()
            .Where(p => p.TenantId == tenantId && p.Invoice.TenantSubscription.SolutionId == solutionId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SubscriptionPayment>> GetByTenantAndDateRangeAsync(
        int tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        => await WithDetails()
            .Where(p => p.TenantId == tenantId && p.PaymentDate >= startDate && p.PaymentDate <= endDate)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SubscriptionPayment>> GetByTenantAndInvoiceIdAsync(
        int tenantId, int invoiceId, CancellationToken cancellationToken = default)
        => await WithDetails()
            .Where(p => p.TenantId == tenantId && p.SubscriptionInvoiceId == invoiceId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
}

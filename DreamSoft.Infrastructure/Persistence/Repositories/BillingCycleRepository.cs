using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class BillingCycleRepository(ApplicationDbContext context)
    : AuditableRepository<BillingCycle>(context), IBillingCycleRepository
{
}

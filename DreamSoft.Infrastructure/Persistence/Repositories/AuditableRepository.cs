using DreamSoft.Domain.Common;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class AuditableRepository<T>(ApplicationDbContext context) : Repository<T>(context), IAuditableRepository<T> where T : AuditableEntity
{
    public virtual async Task<IReadOnlyList<T>> GetAllActiveAsync(CancellationToken cancellationToken = default)
        => await _dbSet.Where(e => e.IsActive).ToListAsync(cancellationToken);
}
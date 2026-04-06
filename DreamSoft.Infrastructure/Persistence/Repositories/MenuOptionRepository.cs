using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class MenuOptionRepository(ApplicationDbContext context)
    : Repository<MenuOption>(context), IMenuOptionRepository
{
    public async Task<MenuOption?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(m => m.Code == code.ToUpper().Trim(), cancellationToken);
}

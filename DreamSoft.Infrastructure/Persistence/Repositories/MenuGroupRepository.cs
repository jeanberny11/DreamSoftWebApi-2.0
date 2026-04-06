using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class MenuGroupRepository(ApplicationDbContext context)
    : Repository<MenuGroup>(context), IMenuGroupRepository
{
}

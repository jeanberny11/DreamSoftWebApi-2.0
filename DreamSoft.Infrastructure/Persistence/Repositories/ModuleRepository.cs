using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class ModuleRepository(ApplicationDbContext context)
    : Repository<Module>(context), IModuleRepository
{
}

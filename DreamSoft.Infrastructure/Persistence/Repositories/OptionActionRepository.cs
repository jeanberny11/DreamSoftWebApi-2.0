using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class OptionActionRepository(ApplicationDbContext context)
    : Repository<OptionAction>(context), IOptionActionRepository
{
}

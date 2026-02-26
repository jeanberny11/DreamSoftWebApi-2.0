using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class IdTypeRepository(ApplicationDbContext context)
    : Repository<IdType>(context), IIdTypeRepository { }

using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class MunicipalityRepository(ApplicationDbContext context)
    : Repository<Municipality>(context), IMunicipalityRepository { }

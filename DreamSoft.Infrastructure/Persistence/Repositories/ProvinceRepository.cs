using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class ProvinceRepository(ApplicationDbContext context)
    : Repository<Province>(context), IProvinceRepository { }

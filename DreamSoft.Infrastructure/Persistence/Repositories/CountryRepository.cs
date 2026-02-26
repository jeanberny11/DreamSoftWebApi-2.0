using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class CountryRepository(ApplicationDbContext context)
    : Repository<Country>(context), ICountryRepository { }

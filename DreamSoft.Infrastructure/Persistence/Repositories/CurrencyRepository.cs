using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class CurrencyRepository(ApplicationDbContext context)
    : Repository<Currency>(context), ICurrencyRepository { }

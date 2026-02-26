using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class LanguageRepository(ApplicationDbContext context)
    : Repository<Language>(context), ILanguageRepository { }

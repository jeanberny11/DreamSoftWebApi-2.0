using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;

namespace DreamSoft.Infrastructure.Persistence.Repositories;

public class MenuOptionRepository(ApplicationDbContext context)
    : Repository<MenuOption>(context), IMenuOptionRepository { }

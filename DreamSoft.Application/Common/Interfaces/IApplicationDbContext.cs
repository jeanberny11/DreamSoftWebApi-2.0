using DreamSoft.Domain.Entities;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;


namespace DreamSoft.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // Core entities
    
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Language> Languages { get; }
    DbSet<Gender> Genders { get; }
    DbSet<IdType> IdTypes { get; }

    // Database operations
    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
}
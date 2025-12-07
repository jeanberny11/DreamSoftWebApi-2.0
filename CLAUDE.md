# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Architecture

This is an ASP.NET Core Web API following **Clean Architecture** principles with four distinct layers:

- **DreamSoft.Domain**: Core domain entities and business rules. Contains no dependencies on other layers. Only has MediatR.Contracts reference.
- **DreamSoft.Application**: Application business logic, CQRS patterns using MediatR, DTOs, mappings (AutoMapper), and validation (FluentValidation). Depends only on Domain.
- **DreamSoft.Infrastructure**: Data access implementation with Entity Framework Core. Supports both SQL Server and PostgreSQL providers. Depends on Application and Domain.
- **DreamSoft.Api**: Web API presentation layer with controllers and Swagger. Entry point of the application. Depends on Application and Infrastructure.

**Dependency Flow**: Api → Infrastructure → Application → Domain

## Key Technologies

- **.NET 8.0** with nullable reference types and implicit usings enabled
- **Entity Framework Core 9.0.11** with SQL Server and PostgreSQL support
- **MediatR 13.1.0** for CQRS pattern implementation
- **AutoMapper 15.1.0** for object-to-object mapping
- **FluentValidation 12.1.0** for request validation
- **Swashbuckle** for API documentation

## Common Commands

### Build and Run
```bash
# Restore dependencies
dotnet restore

# Build entire solution
dotnet build

# Build specific project
dotnet build DreamSoft.Api/DreamSoft.Api.csproj

# Run the API
dotnet run --project DreamSoft.Api

# Run with specific launch profile
dotnet run --project DreamSoft.Api --launch-profile https
```

### Database Migrations (Entity Framework Core)
```bash
# Add a new migration (from solution root)
dotnet ef migrations add MigrationName --project DreamSoft.Infrastructure --startup-project DreamSoft.Api

# Update database to latest migration
dotnet ef database update --project DreamSoft.Infrastructure --startup-project DreamSoft.Api

# Remove last migration (if not applied)
dotnet ef migrations remove --project DreamSoft.Infrastructure --startup-project DreamSoft.Api

# Generate SQL script
dotnet ef migrations script --project DreamSoft.Infrastructure --startup-project DreamSoft.Api
```

### Development
The API runs on:
- HTTPS: https://localhost:7280
- HTTP: http://localhost:5063
- Swagger UI: https://localhost:7280/swagger (in Development environment)

## Project Structure Notes

- Solution has designated folders: `src` (contains all four projects) and `tests` (empty, ready for test projects)
- Each project targets .NET 8.0 with ImplicitUsings and Nullable enabled
- The Api project includes project references to both Application and Infrastructure to wire up dependency injection
- Infrastructure contains the EF Core DbContext and should be the target for all migration commands

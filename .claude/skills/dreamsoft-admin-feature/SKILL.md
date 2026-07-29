---
name: dreamsoft-admin-feature
description: "Generates a complete new AdminApp back-end feature for the DreamSoftWebApi project. Trigger this skill whenever the user says 'Add New Admin Feature', asks to scaffold a new admin feature, or wants to create a new CQRS feature in the AdminApp section. This skill generates all required files following the project's Clean Architecture + CQRS pattern - DTOs, commands, queries, validators, controller, and updates to ErrorMessageKeys.cs and both .resx localization files. Always use this skill for any AdminApp feature scaffolding request, even if the user only partially describes the feature."
---

# DreamSoft AdminApp Feature Generator

This skill scaffolds a complete new AdminApp back-end feature for the DreamSoftWebApi project located at:
`C:\Users\ingbe\Documents\Projects\C# Projects\DreamSoftWebApi`

## Architecture Overview

The project follows **Clean Architecture + CQRS (MediatR)**. Every feature is made of:
- Application layer: `DreamSoft.Application\Features\Apps\AdminApp\{Feature}\`
- API layer: `DreamSoft.Api\Controllers\Apps\AdminApp\`
- Shared localization: `DreamSoft.Application\Common\Exceptions\ErrorMessageKeys.cs` + two `.resx` files

---

## Step 1 — Gather Feature Information

Before generating anything, ask the user for these details if not already provided:

1. **Feature name** — PascalCase singular noun, e.g. `Country`, `DocumentType`
2. **Fields** — name, type, whether required, max length (if string)
3. **Has a unique Code field?** — most AdminApp entities have a `Code` that must be unique; confirm yes/no
4. **Supports translations?** — does the entity use `TranslationsDto` / `TranslatedString` (like Genders/Languages) or plain string fields (like Currencies)?
5. **Any foreign key relationships?** — e.g., belongs to a `Solution`, `Module`, etc.
6. **Active/Inactive filter?** — confirm whether `GetActive{Feature}s` endpoint is needed (default: yes)

Confirm the collected info with the user before generating any files.

---

## Step 2 — Plan the Files to Generate

For a feature named `{Feature}` (e.g., `Country`), the skill generates these files:

### Application Layer — `DreamSoft.Application\Features\Apps\AdminApp\{Feature}s\`

```
{Feature}s/
├── DTOs/
│     └── {Feature}Dto.cs
├── Create{Feature}/
│     ├── Create{Feature}Command.cs
│     ├── Create{Feature}CommandHandler.cs
│     └── Create{Feature}CommandValidator.cs
├── Get{Feature}s/
│     ├── Get{Feature}sQuery.cs
│     └── GetActive{Feature}sQuery.cs
├── Get{Feature}ById/
│     └── Get{Feature}ByIdQuery.cs
└── Update{Feature}/
      ├── Update{Feature}Command.cs
      ├── Update{Feature}CommandHandler.cs
      └── Update{Feature}CommandValidator.cs
```

### API Layer — `DreamSoft.Api\Controllers\Apps\AdminApp\`

```
{Feature}sController.cs
```

### Cross-cutting files to UPDATE (not create):

| File | Change |
|------|--------|
| `DreamSoft.Application\Common\Exceptions\ErrorMessageKeys.cs` | Add `{Feature}CodeAlreadyExists` constant |
| `DreamSoft.Api\Resources\ErrorMessages.resx` | Add English entry |
| `DreamSoft.Api\Resources\ErrorMessages.es.resx` | Add Spanish entry |

---

## Step 3 — Generate Files

Use the templates below. Replace all `{Feature}` / `{feature}` / `{FEATURE}` placeholders with the actual name.

### Naming conventions
- `{Feature}` = PascalCase singular, e.g. `Country`
- `{Features}` = PascalCase plural, e.g. `Countries`
- `{feature}` = camelCase singular, e.g. `country`
- `{feature noun}` = lowercase readable, e.g. `country`
- Namespace root: `DreamSoft.Application.Features.Apps.AdminApp.{Features}`
- Controller namespace: `DreamSoft.Api.Controllers.Apps.AdminApp`

---

### FILE: `DTOs/{Feature}Dto.cs`

```csharp
namespace DreamSoft.Application.Features.Apps.AdminApp.{Features}.DTOs;

public record {Feature}Dto(
    int Id,
    // ← include all fields that make sense to return to the client
    bool IsActive);
```

The DTO is a `record` with all display fields plus `IsActive`. It lives in its own file so all handlers can import it cleanly without circular dependencies.

---

### FILE: `Create{Feature}/Create{Feature}Command.cs`

```csharp
using DreamSoft.Application.Features.Apps.AdminApp.{Features}.DTOs;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.{Features}.Create{Feature};

public record Create{Feature}Command(
    // ← fields required to create the entity
) : IRequest<{Feature}Dto>;
```

---

### FILE: `Create{Feature}/Create{Feature}CommandHandler.cs`

```csharp
using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Features.Apps.AdminApp.{Features}.DTOs;
using DreamSoft.Domain.Entities;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.{Features}.Create{Feature};

public class Create{Feature}CommandHandler(
    I{Feature}Repository {feature}Repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<Create{Feature}Command, {Feature}Dto>
{
    public async Task<{Feature}Dto> Handle(
        Create{Feature}Command request,
        CancellationToken cancellationToken)
    {
        // Duplicate check (only when entity has a unique Code field)
        var exists = await {feature}Repository.AnyAsync(
            x => x.Code == request.Code.ToUpper().Trim(), cancellationToken);

        if (exists)
            throw new ConflictException(ErrorMessageKeys.{Feature}CodeAlreadyExists, request.Code);

        var {feature} = {Feature}.Create(/* map from request */);

        await {feature}Repository.AddAsync({feature}, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new {Feature}Dto(/* map from entity */);
    }
}
```

**Key rules:**
- Always use `ErrorMessageKeys.{Feature}CodeAlreadyExists` — never a raw string
- Call `request.Code.ToUpper().Trim()` on code comparison
- If the entity has foreign key lookups (e.g., `SolutionId`), fetch and throw `NotFoundException` before the conflict check

---

### FILE: `Create{Feature}/Create{Feature}CommandValidator.cs`

```csharp
using FluentValidation;

namespace DreamSoft.Application.Features.Apps.AdminApp.{Features}.Create{Feature};

public class Create{Feature}CommandValidator : AbstractValidator<Create{Feature}Command>
{
    public Create{Feature}CommandValidator()
    {
        // Add a RuleFor per field:
        // Required strings → NotEmpty + MaximumLength
        // Optional strings → MaximumLength only, wrapped in .When(x => x.Field is not null)
        // Translations → validate Spanish.Name as required, English.Name as optional
    }
}
```

---

### FILE: `Get{Features}/Get{Features}Query.cs`

```csharp
using DreamSoft.Application.Features.Apps.AdminApp.{Features}.DTOs;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.{Features}.Get{Features};

public record Get{Features}Query : IRequest<IReadOnlyList<{Feature}Dto>>;

public class Get{Features}QueryHandler(I{Feature}Repository {feature}Repository)
    : IRequestHandler<Get{Features}Query, IReadOnlyList<{Feature}Dto>>
{
    public async Task<IReadOnlyList<{Feature}Dto>> Handle(
        Get{Features}Query request,
        CancellationToken cancellationToken)
    {
        var items = await {feature}Repository.GetAllAsync(cancellationToken);
        return items.Select(x => new {Feature}Dto(/* map */)).ToList();
    }
}
```

If the feature has translations, inject `IRequestLanguageService languageService` and call `languageService.Resolve(request.Language)`.

---

### FILE: `Get{Features}/GetActive{Features}Query.cs`

Same structure as `Get{Features}Query.cs` but:
- Record name: `GetActive{Features}Query`
- Handler calls: `{feature}Repository.GetAllActiveAsync(cancellationToken)`
- This query is `[AllowAnonymous]` — it exists for public tenant-facing use

---

### FILE: `Get{Feature}ById/Get{Feature}ByIdQuery.cs`

```csharp
using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Application.Features.Apps.AdminApp.{Features}.DTOs;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.{Features}.Get{Feature}ById;

public record Get{Feature}ByIdQuery(int Id) : IRequest<{Feature}Dto>;

public class Get{Feature}ByIdQueryHandler(I{Feature}Repository {feature}Repository)
    : IRequestHandler<Get{Feature}ByIdQuery, {Feature}Dto>
{
    public async Task<{Feature}Dto> Handle(
        Get{Feature}ByIdQuery request,
        CancellationToken cancellationToken)
    {
        var {feature} = await {feature}Repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "{Feature}", request.Id);

        return new {Feature}Dto(/* map */);
    }
}
```

---

### FILE: `Update{Feature}/Update{Feature}Command.cs`

```csharp
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.{Features}.Update{Feature};

// Returns Unit (MediatR void) — controller returns 204 No Content
public record Update{Feature}Command(
    int Id,
    // ← updatable fields + IsActive
) : IRequest<Unit>;
```

Note: Update commands return `Unit`, not a DTO. This removes the need to resolve a language on the response and keeps PUT semantics clean.

---

### FILE: `Update{Feature}/Update{Feature}CommandHandler.cs`

```csharp
using DreamSoft.Application.Common.Exceptions;
using DreamSoft.Domain.Repositories;
using MediatR;

namespace DreamSoft.Application.Features.Apps.AdminApp.{Features}.Update{Feature};

public class Update{Feature}CommandHandler(
    I{Feature}Repository {feature}Repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<Update{Feature}Command, Unit>
{
    public async Task<Unit> Handle(
        Update{Feature}Command request,
        CancellationToken cancellationToken)
    {
        var {feature} = await {feature}Repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageKeys.NotFound, "{Feature}", request.Id);

        {feature}.UpdateDetails(/* map from request */);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
```

---

### FILE: `Update{Feature}/Update{Feature}CommandValidator.cs`

Same pattern as Create validator but without `Code` (code is immutable after creation). Validate all updatable fields.

---

### FILE: `{Feature}sController.cs`

```csharp
using DreamSoft.Application.Features.Apps.AdminApp.{Features}.Create{Feature};
using DreamSoft.Application.Features.Apps.AdminApp.{Features}.DTOs;
using DreamSoft.Application.Features.Apps.AdminApp.{Features}.Get{Features};
using DreamSoft.Application.Features.Apps.AdminApp.{Features}.Get{Feature}ById;
using DreamSoft.Application.Features.Apps.AdminApp.{Features}.Update{Feature};
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DreamSoft.Api.Controllers.Apps.AdminApp;

public class {Features}Controller : AdminControllerBase
{
    // ── GET /api/v1/admin/{features} ──────────────────────────────────────────
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<{Feature}Dto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new Get{Features}Query(), cancellationToken));

    // ── GET /api/v1/admin/{features}/active ───────────────────────────────────
    [HttpGet("active")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<{Feature}Dto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllActive(CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new GetActive{Features}Query(), cancellationToken));

    // ── GET /api/v1/admin/{features}/{id} ────────────────────────────────────
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof({Feature}Dto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        => Ok(await Mediator.Send(new Get{Feature}ByIdQuery(id), cancellationToken));

    // ── POST /api/v1/admin/{features} ─────────────────────────────────────────
    [HttpPost]
    [ProducesResponseType(typeof({Feature}Dto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] Create{Feature}Command command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, version = "1" }, result);
    }

    // ── PUT /api/v1/admin/{features}/{id} ────────────────────────────────────
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] Update{Feature}Request body,
        CancellationToken cancellationToken)
    {
        await Mediator.Send(new Update{Feature}Command(id, /* map body fields */), cancellationToken);
        return NoContent();
    }
}

// ── Request body DTO ──────────────────────────────────────────────────────────

public record Update{Feature}Request(
    // ← same fields as Update{Feature}Command minus Id
);
```

**Key rules:**
- `Create` uses `CreatedAtAction(nameof(GetById), ...)` — provides proper REST `Location` header
- `Update` returns `NoContent()` (204) — no DTO needed, no language ambiguity
- `Update{Feature}Request` is defined at the bottom of the controller file — it separates the route `id` from the body

---

### Cross-cutting updates

#### `ErrorMessageKeys.cs` — add inside the `// ── Conflict / Already-done` region:

```csharp
public const string {Feature}CodeAlreadyExists = "{Feature}CodeAlreadyExists";
```

Place it alphabetically or grouped with other feature-specific conflict keys.

#### `ErrorMessages.resx` — add before the closing `</root>`:

```xml
<data name="{Feature}CodeAlreadyExists" xml:space="preserve">
  <value>A {feature noun} with code '{0}' already exists</value>
</data>
```

#### `ErrorMessages.es.resx` — add before the closing `</root>`:

```xml
<data name="{Feature}CodeAlreadyExists" xml:space="preserve">
  <value>Ya existe un {feature noun} con el código '{0}'</value>
</data>
```

---

## Step 4 — Translation-aware variants

When the feature uses `TranslationsDto` (like Genders, Languages):

- Inject `IRequestLanguageService languageService` in query handlers
- In `Get{Features}Query`, accept `string? Language = null` parameter
- In `GetActive{Features}Query`, accept `string? Language = null` parameter  
- In `Get{Feature}ByIdQuery`, accept `string? Language = null` parameter
- In `Create{Feature}CommandHandler`, build `TranslatedString` from `request.Translations` before calling `{Feature}.Create(...)`
- In `Update{Feature}CommandHandler`, rebuild `TranslatedString` from `request.Translations` before calling `{feature}.UpdateDetails(...)`
- The controller `GetAll`, `GetAllActive`, `GetById` actions add `[FromQuery] string? language` parameter
- The DTO `Name` field is resolved via `translations.GetNameOrFallback(language, entity.Name)`

When the feature does NOT use translations (like Currencies), omit all of the above.

---

## Step 5 — Checklist Before Finishing

Before presenting the files to the user, verify each item:

- [ ] `{Feature}Dto.cs` is in its own `DTOs/` folder
- [ ] All handlers import DTO from `DTOs/` namespace — not from a Query file
- [ ] `Get{Features}Query.cs` and `GetActive{Features}Query.cs` are separate files
- [ ] `Create{Feature}Command` returns `IRequest<{Feature}Dto>`
- [ ] `Update{Feature}Command` returns `IRequest<Unit>` — no DTO on update
- [ ] `Create` controller action uses `CreatedAtAction(nameof(GetById), ...)`
- [ ] `Update` controller action returns `NoContent()` (204)
- [ ] `ConflictException` uses `ErrorMessageKeys.{Feature}CodeAlreadyExists` — no raw strings
- [ ] `ErrorMessageKeys.cs` has the new constant added
- [ ] `ErrorMessages.resx` has the English entry added
- [ ] `ErrorMessages.es.resx` has the Spanish entry added
- [ ] If translations: `IRequestLanguageService` injected in all query handlers
- [ ] If no translations: no `IRequestLanguageService` anywhere in the feature

---

## Examples

**Simple feature (no translations) — like Currencies:**
- Fields: `Code (string, required, max 10)`, `Name (string, required, max 100)`, `IsDefault (bool)`
- Has Code uniqueness check: yes
- Translations: no
- Active filter: yes

**Translated feature — like Genders:**
- Fields: `Code (string, required, max 50)`, `Name (string, required, max 100)`, `Translations (TranslationsDto, required)`
- Has Code uniqueness check: yes
- Translations: yes
- Active filter: yes

**Relational feature — like SubscriptionPlans:**
- Fields: `Code`, `Name`, `Translations`, `SolutionId (int, FK)`, `TierLevel (int)`, `TrialDays (int)`, `Description (string?, optional)`
- Has Code uniqueness check: yes
- Translations: yes
- FK lookup: fetch `Solution` by `SolutionId`, throw `NotFoundException` if not found
- Active filter: yes (but no `[AllowAnonymous]` on active endpoint for this feature type)

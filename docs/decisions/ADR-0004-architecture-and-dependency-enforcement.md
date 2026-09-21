# ADR-0004: Architecture and dependency enforcement

**Status:** Accepted

## Context

GymTron follows a Clean/Hexagonal Architecture where dependencies should point inward toward the Domain. However:
- No automated checks existed to enforce architectural boundaries or prevent dependency drift.
- `GymTron.Domain` references external packages (`MediatR` and `Microsoft.Extensions.Logging.Abstractions`), and their permitted usage needed an explicit boundary ceiling.
- Presentation layers (`GymTron.Web` and `GymTron.App`) directly reference `GymTron.Infrastructure` in their project files. This reference is required as a Composition Root to register persistence services into the dependency injection container, but risks leaking persistence details directly into presentation components (e.g. PageModels, ViewModels).

## Decision

1. **Permitted Dependency Directions**:
   - `GymTron.Domain`: Must not depend on `GymTron.Application`, `GymTron.Infrastructure`, `GymTron.Api`, `GymTron.Web`, or `GymTron.App`. It must have zero references to database drivers (`MySql.Data`), ORMs/micro-ORMs (`Dapper`), or UI frameworks.
   - `GymTron.Application`: Depends solely on `GymTron.Domain`. Must not depend on `GymTron.Infrastructure`, `GymTron.Api`, `GymTron.Web`, or `GymTron.App`. It must have zero references to persistence or UI libraries.
   - `GymTron.Infrastructure`: Depends on `GymTron.Domain` to implement its repository contracts. Must not depend on `GymTron.Application`, `GymTron.Api`, `GymTron.Web`, or `GymTron.App`.
   - `GymTron.Web` and `GymTron.Api`: Act as Composition Roots. They reference `GymTron.Application`, and reference `GymTron.Infrastructure` strictly for DI service registration in `Program.cs`. Direct consumption of DAL classes, raw SQL models, or Infrastructure types inside ViewModels or Endpoints is prohibited.
   - `GymTron.App`: Decoupled mobile client. It has zero project references to `GymTron.Domain`, `GymTron.Application`, or `GymTron.Infrastructure`. It communicates solely with `GymTron.Api` over HTTP via `IGymTronApiClient`.

2. **Domain Package Policy**:
   - The allowed external packages in `GymTron.Domain` are frozen to `MediatR` (for Domain events) and `Microsoft.Extensions.Logging.Abstractions` (for logging abstractions).
   - No additional third-party dependencies may be introduced into Domain without an approved ADR.

3. **Automated Enforcement**:
   - Introduce an automated architecture test suite under `tests/GymTron.UnitTests/Architecture/ArchitectureTests.cs` using `NetArchTest.Rules` and reflection assertions.
   - Run these checks as part of the standard `dotnet test` suite and CI pipeline.

## Consequences

### Positive
- Prevents architectural regressions and circular or inverted dependencies at build and test time.
- Clarifies the role of composition roots vs. presentation code.
- Eliminates ambiguity regarding which external packages are permissible within `GymTron.Domain`.

### Negative
- Adds a test dependency (`NetArchTest.Rules`) to `GymTron.UnitTests`.
- Developers must respect established layer boundaries or update architectural rules intentionally through ADRs.

### Risks
- **Risk**: Presentation code unintentionally uses Infrastructure internal types because of the project reference.
  - **Mitigation**: Infrastructure internal contracts remain `internal` (using `InternalsVisibleTo` only for test assemblies and dynamic proxy generation), preventing compile-time leakage into presentation projects.
- **Risk**: CI or local builds fail if architecture tests are slow.
  - **Mitigation**: NetArchTest / reflection rules analyze loaded assembly metadata in memory in less than 50 milliseconds.

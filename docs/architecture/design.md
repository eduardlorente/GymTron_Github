# Architecture Design

## Purpose and scope

This document describes the repository as observed. It separates current structure from desired capabilities; it does not promote legacy claims into hard invariants.

## Confirmed module map

| Project | Target | Observed responsibility and dependencies |
|---|---|---|
| `GymTron.Domain` | `net8.0` | Domain entities, projections, repository contracts, and services. References MediatR and logging abstractions. |
| `GymTron.Application` | `net8.0` | MediatR commands/queries and orchestration. References Domain. |
| `GymTron.Infrastructure` | `net8.0` | MySQL/Dapper repositories and DAL implementations. References Domain. |
| `GymTron.Api` | Minimal API `net10.0` | REST API composition root and Native REPR endpoints. References Application and Infrastructure. |
| `GymTron.App` | MAUI `net9.0-android`, plus conditional Windows | Mobile client UI and composition root. Zero references to Domain, Application, or Infrastructure; consumes `GymTron.Api` via HTTP (`IGymTronApiClient`). |
| `GymTron.Web` | Razor Pages `net10.0` | Web composition root and UI. References Application and Infrastructure. |

Evidence: `GymTron.sln`, current project files under `src/`, `GymTron.Web/Program.cs`, `GymTron.Api/Program.cs`, `GymTron.App/MauiProgram.cs`, and Infrastructure registration and DAL source. Deleted `MigratorDB` and `Tortuga` projects are not current architecture.

## Observed dependency map

```text
GymTron.App ───────(HTTP/REST)───────> GymTron.Api ───────> Application ───────> Domain
                                              │                                      ▲
                                              └────────────> Infrastructure ─────────┘

GymTron.Web ──────> Application
     └────────────> Infrastructure ───────> Domain
```

The Web and API references to Infrastructure are used exclusively for composition-root registration. `GymTron.App` has zero direct project references to internal layers and connects via HTTP. Automated architectural boundaries are enforced in `tests/GymTron.UnitTests/Architecture/ArchitectureTests.cs` per `docs/decisions/ADR-0004-architecture-and-dependency-enforcement.md`.

## Entry points and flow

- `src/GymTron.Api/Program.cs` registers Application and Infrastructure services, ProblemDetails, OpenAPI, and native REPR endpoint groups.
- `src/GymTron.App/MauiProgram.cs` configures `IGymTronApiClient` via `HttpClient`, registers App services, view models, and pages.
- `src/GymTron.Web/Program.cs` obtains `DefaultConnection`, registers Razor Pages, Application, and Infrastructure, then configures the HTTP pipeline.

The primary observed application path is presentation (Web directly, MAUI via API HTTP) -> Application/MediatR -> Domain repository contracts -> Infrastructure repositories/DAL -> MySQL. Some exception logging also dispatches through MediatR.

## Layer implementation patterns

### Domain

- `Entity<TId>` and `AggregateRoot<TId>` provide identity, `EntityStatus`, and accumulated domain events. Entities mostly restrict mutation through private or protected setters, but mutable collections are publicly exposed in places; strict aggregate encapsulation is not established.
- The project owns a custom `ValueObject<T>` model and keeps repository contracts in Domain.
- Static construction and rehydration are common, with names including `New`, `Create...`, and `FromDatabase`. There is no universal factory vocabulary.
- Domain events are collected and explicitly published through MediatR, with limited adoption. Domain currently depends on MediatR and logging abstractions.
- Generic exceptions coexist with domain-specific exceptions. Domain defines `DomainException` as the base for expected business failures, with `EntityNotFoundException` for missing entities and `InvalidDomainOperationException` for state-transition and business-rule violations. Repository contracts were narrowed to the operations Application actually uses; unsupported members no longer throw `NotImplementedException`. Neither generic business exceptions nor a repository-wide CRUD contract is an approved convention.

### Application

- Features are organized around command/query request types and handlers. Base request types carry correlation identifiers.
- Internal handlers commonly inherit shared command/query handler bases that catch exceptions, log through `IExceptionLogger`, and rethrow. `RegisterLogCommandHandler` directly implements `IRequestHandler` and is an observed exception.
- FluentValidation coverage is complete for all commands and queries with input parameters. A MediatR pipeline behavior (`ValidationBehavior`) executes registered validators before handlers, throwing `ValidationException` for invalid requests. Validators are registered via DI (`AddValidatorsFromAssembly`) and are no longer instantiated manually in handlers. Command/query request types and their validators share one file. DTOs live in `Commands/DTO` or `Queries/DTO` subfolders with a matching `.DTO` sub-namespace (e.g. `GymTron.Application.Routines.Queries.DTO`). Domain projections (`*HistoryProjection`) remain in Domain and are not Application DTOs.
- Handler cancellation tokens are propagated through repository contracts, Infrastructure repositories, DAL interfaces, and Dapper calls end to end.

### Persistence

- Public Domain repository contracts are implemented by internal Infrastructure repositories, which depend on internal DAL contracts, DAL implementations, and persistence models.
- Repositories commonly reconstruct Domain objects, including through `FromDatabase`, but mapping is not universal.
- Dapper and `MySql.Data` execute SQL embedded in DAL methods, generally with parameterized runtime values and a new connection per operation.
- Memory caching is isolated to routine reads. Multi-step routine writes now run inside a single connection and transaction in `RoutineDAL`, but rollback has not been proven against MySQL yet; the read-uncommitted SQL wrapper also has unverified reset behavior.

See `docs/infra/configuration-and-delivery.md` for persistence risks and operational evidence.

## Frontend implementation summary

- `GymTron.App` follows a page/XAML -> view model -> App service -> `IGymTronApiClient` (HTTP) -> `GymTron.Api` flow. It uses manual `INotifyPropertyChanged`, MAUI commands, mixed navigation and lifecycle approaches, and binding-heavy XAML.
- `GymTron.Web` uses feature-oriented Razor Pages. Feature `PageModel` classes dispatch Application requests through MediatR; editable pages use bound Web view models, DataAnnotations, and `ModelState`.

Frontend details, inconsistencies, and pending work are canonical in `docs/frontend/README.md`, `docs/frontend/maui.md`, and `docs/frontend/web.md`.

## Persistence and integrations

MySQL is the active persistence technology. Infrastructure uses Dapper with `MySql.Data`. A query extension changes transaction isolation around SQL text, but failure/reset behavior has not been proven. The previously dormant Google Sheets integration (packages, source, and `IResourceProvider`/`ResourceProvider` helpers) was removed; MySQL is the sole persistence backend.

## Time Semantics

All timestamps in the domain layer are stored in UTC. The `IClock` interface (`GymTron.Domain.Services.IClock`) provides:
- `UtcNow`: Returns current time in UTC — use for all persistence and domain logic.
- `Now`: Returns current time in local timezone — use for display only.

**Rules:**
1. Domain entities and events always use `clock.UtcNow`.
2. Application handlers inject `IClock` and pass it to domain methods.
3. Infrastructure registers `SystemClock` as the production `IClock` singleton.
4. Tests use `FakeClock` (`GymTron.UnitTests.Helpers.FakeClock`) for deterministic, controllable time.
5. UI layer (MAUI) may use `DateTime.Now` for elapsed-time display only.

## Guidance decision tree

1. Changing cross-project structure or dependencies? Read this file and the capability backlog.
2. Changing C#? Read `docs/language/csharp.md` after inspecting the affected project.
3. Changing MAUI or Web presentation code? Read the applicable guide under `docs/frontend/`. Changing API endpoints or REST contracts? Read `docs/api/README.md`.
4. Changing configuration, persistence, integration, migration, or deployment? Read `docs/infra/configuration-and-delivery.md`.
5. Running or validating the repository? Read `docs/onboarding/getting-started.md`.
6. Making a durable cross-cutting decision? Consult `docs/decisions/README.md` and use the ADR process (`docs/decisions/TEMPLATE.md`).

## Precedence

| Source | Precedence and use |
|---|---|
| Current executable code/configuration plus explicit task requirements | Highest evidence for current behavior; never expose secret values. |
| Accepted ADR | Governs its recorded scope across the repository. Registered in `docs/decisions/README.md`. |
| This global architecture document | Governs documented cross-project understanding, subject to current evidence. |
| Per-change design | Governs only its approved change and cannot silently override global guidance. |
| README or legacy prose | Context only when consistent with current evidence. |
| Undocumented inference | Not policy; verify or mark pending. |

## Rule ownership

| Topic | Canonical owner |
|---|---|
| Agent workflow | `AGENTS.md` |
| Architecture and dependency map | This file |
| C# conventions | `docs/language/csharp.md` |
| API endpoints and REPR patterns | `docs/api/README.md` |
| Frontend patterns and boundaries | `docs/frontend/README.md` and its focused guides |
| Configuration and delivery evidence | `docs/infra/configuration-and-delivery.md` |
| Missing technical capabilities | `docs/requirements/technical-requirements.md` |
| Commands and onboarding | `docs/onboarding/getting-started.md` |
| Durable decisions | `docs/decisions/README.md` |

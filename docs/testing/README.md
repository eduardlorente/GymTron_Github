# Automated Testing

## Current baseline

`tests/GymTron.UnitTests` is the canonical automated-test project. It targets `net10.0`, uses xUnit, NSubstitute, and the Coverlet 6.0.4 collector, and references `GymTron.Domain`, `GymTron.Application`, and `GymTron.Infrastructure`.

`tests/GymTron.IntegrationTests` is a separate MySQL DAL suite using Testcontainers.MySql 4.15.0 and `mysql:8.0`. **Status: Executed and passing.** 17 tests cover all six DALs, repository behaviors, transaction rollback, isolation reset, and nullable mappings. Infrastructure coverage is 82.80% lines and 79.17% branches, exceeding the 60/60 gate.

The suite is intentionally behavior-first:

- Domain tests cover entities and factories, aggregate behavior and rehydration, status transitions, value-object equality, domain events and publication, exceptions, and projections.
- Application tests cover request correlation and pagination contracts; shared handler success and log/rethrow behavior; exception-log delegation; training start, completion, cancellation, exercise completion, cancellation-token forwarding, current/history queries, and validation failures; routine and exercise-parameter command mapping and query projections; body-weight registration/history; exercise history; log registration; validation pipeline behavior; and validator coverage for commands and queries.
- Infrastructure tests cover service registrations and DAL factories without opening connections; repository delegation, null and empty paths, source ordering and routine grouping; routine cache hits and write invalidation; DAL model properties; query wrapping; and body-weight, exercise, exercise-parameter, routine, and training mappings.
- Architecture tests under `Architecture/` enforce Clean Architecture boundary rules, external package limits, and MSBuild project reference directions using NetArchTest.Rules and reflection.
- NSubstitute is limited to Application boundaries where repository, mediator, and logger interactions carry behavioral meaning.

Test classes are grouped by production boundary under `Domain/`, `Application/`, and `Architecture/`. Test names use `Operation_State_ExpectedBehavior` so failures describe the behavior that regressed.

## Running the suite

The workstation-level NuGet configuration currently includes an inaccessible private feed. On a clean restore, restore the test project from the public source explicitly:

```powershell
dotnet restore tests/GymTron.UnitTests/GymTron.UnitTests.csproj --source https://api.nuget.org/v3/index.json
```

After a successful restore, build and run the suite without another restore:

```powershell
dotnet build tests/GymTron.UnitTests/GymTron.UnitTests.csproj --configuration Release --no-restore
dotnet test tests/GymTron.UnitTests/GymTron.UnitTests.csproj --configuration Release --no-build --no-restore --settings tests/coverage.runsettings --collect "XPlat Code Coverage" --logger trx --results-directory TestResults
& ./eng/Assert-Coverage.ps1 -CoveragePath "TestResults/**/coverage.cobertura.xml" -Threshold @("GymTron.Domain=100,100", "GymTron.Application=80,80")
```

The Application and Infrastructure projects grant `GymTron.UnitTests` internal visibility through SDK-supported `InternalsVisibleTo` project items. Infrastructure also grants visibility to NSubstitute's `DynamicProxyGenAssembly2` so internal DAL contracts can remain internal while being substituted.

## Continuous integration

`.github/workflows/validation.yml` runs two jobs on `ubuntu-latest`:

1. **unit-tests**: restores, builds, and runs `GymTron.UnitTests` with coverage, enforcing Domain 100%/100% and Application 80%/80%.
2. **integration-tests**: restores, builds, and runs `GymTron.IntegrationTests` with coverage, enforcing Infrastructure 60%/60%. This job requires Docker (available on ubuntu-latest) for Testcontainers.

Both jobs consume checked-in assets only and upload their respective `TestResults` artifacts even when testing or coverage enforcement fails. The workflow configuration is present; hosted Ubuntu results are available only after GitHub Actions executes them.

## Coverage configuration and enforcement

`tests/coverage.runsettings` measures the production assemblies `GymTron.Domain`, `GymTron.Application`, and `GymTron.Infrastructure`. It excludes only compiler-generated file patterns and `obj` output. Domain and Application are enforced; Infrastructure remains measurable for its separately scoped target.

`eng/Assert-Coverage.ps1` reads Cobertura package identities and derives covered/valid line and branch counts from package line records. It fails for missing files, packages, or metrics, and compares integer counts rather than rounded display percentages. Pass one or more independent thresholds as `Assembly=LinePercent,BranchPercent`; for example, future gates can add `GymTron.Application=80,80` and `GymTron.Infrastructure=60,60` without changing the script.

The verified Release run passed 107 unit tests and 17 integration tests. Exact coverage was 284/284 lines and 36/36 branches for Domain (100%/100%), 412/435 lines and 33/34 branches for Application (94.71%/97.06%), and 496/599 lines and 19/24 branches for Infrastructure (82.80%/79.17%). All three gates pass: Domain 100%/100%, Application 80%/80%, and Infrastructure 60%/60%.

The Domain project grants the test assembly internal visibility for deterministic lifecycle verification. `EntityStatus` retains its existing `DateTime.Now` production entry points and delegates to internal timestamp overloads; public behavior and the unresolved time-policy gap are unchanged. An unreachable, unused private `BodyWeight` constructor was removed.

## MySQL integration suite

Prerequisites are a supported Docker-compatible engine with a healthy daemon, permission to pull/use `mysql:8.0`, and access to NuGet.org for restore. The collection fixture creates one disposable MySQL container, generates a unique database, user, and password for that run, waits for module readiness, applies only `tests/GymTron.IntegrationTests/Database/schema.sql`, resets all seven test tables before each test, and disposes the container after the collection. It never reads `.env`, production credentials, application secret files, or the root `init.sql`.

Restore and compile without starting Docker:

```powershell
dotnet restore tests/GymTron.IntegrationTests/GymTron.IntegrationTests.csproj --source https://api.nuget.org/v3/index.json
dotnet build tests/GymTron.IntegrationTests/GymTron.IntegrationTests.csproj --configuration Release --no-restore
```

Run the integration suite and collect its separate results:

```powershell
dotnet test tests/GymTron.IntegrationTests/GymTron.IntegrationTests.csproj --configuration Release --no-build --no-restore --settings tests/integration.coverage.runsettings --collect "XPlat Code Coverage" --logger trx --results-directory tests/GymTron.IntegrationTests/TestResults-<run-id>
```

The integration suite exercises all six MySQL DALs, repository behaviors (including cache invalidation and null paths), transaction rollback (FK violation mid-write), isolation reset (after success and failure), and nullable mappings. Two `ExerciseDAL` defects were confirmed and fixed during the first authorized run: `Add` now maps domain properties to SQL parameters correctly (aligning with `AddRange`), and `ListAll` now selects `exercise_parameters_id` so that `ExerciseDALModel.ExerciseParametersId` is populated.

## Web PageModel test suite

`tests/GymTron.Web.Tests` targets `net10.0` and uses xUnit and NSubstitute to verify `GymTron.Web` `PageModel` classes (including Routines and ExerciseParameters index, details, creation, and edition).

```powershell
dotnet test tests/GymTron.Web.Tests/GymTron.Web.Tests.csproj --configuration Release
```

## Remaining coverage boundary

This baseline proves focused in-process Domain, Application, and Infrastructure repository/mapping behaviors, real-MySQL DAL transactions and repository behaviors through the integration suite, and Web PageModel interaction flows.

It does not prove:

- Web HTTP routing, middleware, Razor rendering, or full browser behavior;
- MAUI startup, navigation, binding, lifecycle, platform behavior, or device execution (MAUI UI automated testing formally discarded by ADR-0005);
- Docker bootstrap, external services, deployment, or end-to-end critical flows.

Add tests when behavior changes, keeping external systems outside this project. Create separately scoped integration or frontend test projects when those boundaries are approved.

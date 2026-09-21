# Getting Started Safely

The supported build baseline is verified on Windows x64. It uses the .NET SDK selected by the root `global.json`; Linux and macOS support have not been verified.

## Read order

1. Read `POINTS_OF_TRUTH.md` and `AGENTS.md`.
2. Read `README.md` for preserved product context, treating claims that conflict with executable configuration as stale.
3. Read `docs/architecture/design.md` for current projects and dependencies.
4. Load the focused language, infrastructure, or requirements document relevant to the task.
5. Inspect the affected code/configuration before changing it.

## Required SDK baseline

Install the .NET `10.0.301` SDK. The root `global.json` selects that SDK with `rollForward: latestPatch` and excludes prerelease SDKs. This policy permits servicing patches only within the `10.0.3xx` feature band instead of moving contributors to a different feature band implicitly.

The .NET 10 SDK is required for `GymTron.Web` (`net10.0`) and, in the verified environment, also builds the `net8.0` libraries and `net9.0` MAUI targets. Confirm selection from the repository root:

```powershell
dotnet --version
```

Expected result: `10.0.301`, or a later `10.0.3xx` servicing patch if one is installed.

## Required workloads

| Target | Required workload | Host scope |
|---|---|---|
| `net9.0-android` | `android` | Verified on Windows x64. |
| `net9.0-windows10.0.19041.0` | `maui-windows` | Conditional project target; available and applicable only on Windows hosts. |

Check installed workloads without changing the machine:

```powershell
dotnet workload list
```

The current project does not target iOS or MacCatalyst. Their presence on a workstation is not part of this baseline.

For initial machine setup only, the following command analyzes the MAUI project and installs any missing required workloads in the SDK context selected by `global.json`:

```powershell
dotnet workload restore src/GymTron.App/GymTron.App.csproj
```

This is an **install-affecting bootstrap action**, not a routine validation command. Review its effects before running it; do not run it merely to prove a build.

## Configuration prerequisites

- **Web:** set `ConnectionStrings:DefaultConnection` through ASP.NET Core User Secrets for local development, or through an environment/provider override such as `ConnectionStrings__DefaultConnection`. Do not place a value in tracked JSON. See `docs/infra/configuration-and-delivery.md`.
- **MAUI:** copy `src/GymTron.App/Resources/Json/appsettings.Secrets.example.json` to the ignored `appsettings.Secrets.json` beside it and replace its placeholders locally. Do not commit or document values.

## Verified build matrix

The following evidence was obtained on Windows x64 with SDK `10.0.301`, existing restored packages, Android workload manifest `36.1.43/10.0.100`, and MAUI Windows workload manifest `10.0.20/10.0.100`.

| Project | Target | Verification command | Exact scope and status |
|---|---|---|---|
| `GymTron.Domain` | `net8.0` | Web or Android command below | Built transitively by both verified commands. |
| `GymTron.Application` | `net8.0` | Web or Android command below | Built transitively by both verified commands. |
| `GymTron.Insfrastructure` | `net8.0` | Web or Android command below | Built transitively by both verified commands. |
| `GymTron.Web` | `net10.0` | `dotnet build src/GymTron.Web/GymTron.Web.csproj --no-restore` | Passed with 2 existing nullability warnings and 0 errors; builds Web and its referenced library graph. This is build proof, not startup or behavioral proof. |
| `GymTron.App` | `net9.0-android` | `dotnet build src/GymTron.App/GymTron.App.csproj -f net9.0-android --no-restore -m:1` | Passed with 129 warnings and 0 errors; builds Android and its referenced library graph. This is build proof, not emulator/device or behavioral proof. |
| `GymTron.App` | `net9.0-windows10.0.19041.0` | `dotnet build src/GymTron.App/GymTron.App.csproj -f net9.0-windows10.0.19041.0 --no-restore -m:1` | Passed with 154 warnings and 0 errors on Windows x64. This target is not claimed for other hosts. |

`--no-restore` is appropriate only after package restore has already succeeded for the current lock/assets state. A separate verifier should record actual command output rather than infer success from this matrix.

## Verified automated tests

The focused Domain, Application, and pure Infrastructure suite is documented in `docs/testing/README.md`. Because the current workstation NuGet configuration includes an inaccessible private feed, restore from NuGet.org explicitly:

```powershell
dotnet restore tests/GymTron.UnitTests/GymTron.UnitTests.csproj --source https://api.nuget.org/v3/index.json
```

After that restore succeeds, use the same Release build and test sequence as CI:

```powershell
dotnet build tests/GymTron.UnitTests/GymTron.UnitTests.csproj --configuration Release --no-restore
dotnet test tests/GymTron.UnitTests/GymTron.UnitTests.csproj --configuration Release --no-build --no-restore --settings tests/coverage.runsettings --collect "XPlat Code Coverage" --logger trx --results-directory TestResults
& ./eng/Assert-Coverage.ps1 -CoveragePath "TestResults/**/coverage.cobertura.xml" -Threshold @("GymTron.Domain=100,100", "GymTron.Application=80,80")
```

The verified Release sequence passes 61 tests. It reports and enforces exact `GymTron.Domain` coverage of 280/280 lines and 36/36 branches, plus `GymTron.Application` coverage of 380/383 lines and 25/26 branches against minimum 80% line and branch thresholds. It also measures `GymTron.Insfrastructure` at 279/588 lines and 13/24 branches (47.45%/54.17%), below the 60% line target, so Infrastructure is not yet gated. These commands compile Infrastructure and execute only its pure repository, mapping, model, query-transformation, and registration tests; they do not run Web, MAUI, a database, Docker, or external services.

`.github/workflows/validation.yml` runs this focused sequence on `ubuntu-latest` using the SDK selected by `global.json`, the checked-in test project, coverage settings, and gate script. It gates Domain at 100% line and branch coverage and Application at a minimum 80% line and branch coverage, then uploads test/coverage artifacts. Infrastructure remains measured but not enforced until meaningful MySQL integration tests can raise its line coverage to 60%; a hosted result is established only when GitHub Actions runs the workflow.

### MySQL DAL integration status

`tests/GymTron.IntegrationTests` is **Implemented but not executed**. It uses Testcontainers.MySql 4.15.0 with `mysql:8.0`, runtime-generated non-production credentials, one container per xUnit collection, a minimal test-owned schema, and a table reset before each test. It does not use `.env`, application secrets, or the root `init.sql`.

The project can be restored and compiled without Docker:

```powershell
dotnet restore tests/GymTron.IntegrationTests/GymTron.IntegrationTests.csproj --source https://api.nuget.org/v3/index.json
dotnet build tests/GymTron.IntegrationTests/GymTron.IntegrationTests.csproj --configuration Release --no-restore
```

The future test command, lifecycle, and prerequisites are documented in `docs/testing/README.md`. Do not run it until container execution is explicitly authorized. Integration coverage and the Infrastructure 60%/60% target remain unproven and are not CI gates.

## What remains unverified

- MAUI Windows launch and behavior.
- Web startup and behavior against a configured local database.
- Android emulator/device launch and behavior.
- A whole-solution command as the canonical build entry point.
- Package restore from an empty cache, macOS hosts, Docker execution, publishing, signing, deployment, and rollback.
- Hosted Ubuntu execution of the focused CI workflow remains unverified until GitHub Actions reports its first run.
- Execution evidence for the implemented MySQL DAL integration suite, plus Web, MAUI, and end-to-end critical-flow tests. The focused unit-test baseline does not provide those forms of behavioral verification.

## Unsafe or non-routine actions

- `MigratorDB` and `Tortuga` have been removed from the current solution and supported source scope.
- Do not run `StartDockerScript.ps1` or `docker compose` as routine onboarding until the external environment values are supplied and the bootstrap is validated. Never use a production credential for local setup.
- Do not use committed credential material to test live connectivity.
- Follow `Instruccions generar APK.txt` for signing-secret boundaries; do not store a keystore or password in the repository.

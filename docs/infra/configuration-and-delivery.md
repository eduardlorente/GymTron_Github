# Configuration and Delivery

## Current evidence

- Web configuration is loaded through standard ASP.NET Core providers. Tracked JSON files contain no connection value; local development uses User Secrets, and deployment can use `ConnectionStrings__DefaultConnection` or another provider.
- MAUI layers a tracked non-secret environment JSON resource with an optional-at-build local `appsettings.Secrets.json` embedded resource. Startup fails without the local resource or a non-blank `DefaultConnection`.
- `docker-compose.yml`, `StartDockerScript.ps1`, and `init.sql` describe a local MySQL bootstrap. Database credentials come from external environment input (`MYSQL_ROOT_PASSWORD` required, `MYSQL_DATABASE` defaults to `gymtron`). `StartDockerScript.ps1` reads from `.env` or environment variables, uses `docker compose` (v2), and validates the password variable before starting. The bootstrap has been validated with `docker compose config`.
- MySQL/Dapper persistence is registered by `src/GymTron.Infrastructure/ServiceCollectionExtensions.cs`.
- The root `global.json` selects .NET SDK `10.0.301` with patch-only roll-forward. No CI workflow, deployment verification contract, or rollback process was found.

## Local configuration

### Web

From the repository root, configure the development connection with ASP.NET Core User Secrets. Substitute a local value without recording it in shell output, documentation, or source control:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<connection-string>" --project src/GymTron.Web/GymTron.Web.csproj
dotnet user-secrets list --project src/GymTron.Web/GymTron.Web.csproj
dotnet user-secrets remove "ConnectionStrings:DefaultConnection" --project src/GymTron.Web/GymTron.Web.csproj
```

Hosted environments may set `ConnectionStrings__DefaultConnection` through their secret provider. Web startup rejects a missing or blank value and does not log it.

### MAUI Configuration

`GymTron.App` connects exclusively to `GymTron.Api` via HTTP (`IGymTronApiClient`). It requires an `ApiUrl` entry in its embedded `appsettings.json` / `appsettings.Development.json`. Mobile clients hold zero database credentials or connection strings.

### Docker

Copy `.env.example` to `.env` for direct Docker Compose use, or set `MYSQL_ROOT_PASSWORD`, `MYSQL_DATABASE`, `JWT_SECRET_KEY`, and optionally `MYSQL_PORT`/`API_PORT` in the current process. `.env` files are ignored except for `.env.example`. `StartDockerScript.ps1` loads variables from `.env` if present, validates that `MYSQL_ROOT_PASSWORD` and `JWT_SECRET_KEY` (minimum 32 bytes) are set, uses `docker compose` (v2), and conditionally applies `init.sql` if it exists. The script contains no hardcoded credentials.

### Publishing and signing

- The tracked WebDeploy profile contains reusable MSBuild placeholders only and conditionally imports the ignored `site53146-WebDeploy.local.props`. Supply `GymTronPublishSiteUrl`, `GymTronPublishServiceUrl`, `GymTronPublishIisAppPath`, and `GymTronPublishUserName` there or through CI variables; supply the password only through the publishing tool or CI secret provider.
- `Instruccions generar APK.txt` is the current safe signing guide. `GymTron.App.csproj` conditionally imports the ignored `src/GymTron.App/GymTron.Signing.local.props`; keep keystores, certificates, private keys, signing properties, aliases, and passwords there or in CI secrets.

## Current persistence implementation

The active application persistence path is Domain repository contract -> internal Infrastructure repository -> internal DAL contract and implementation -> Dapper/`MySql.Data` -> MySQL.

- Infrastructure registers repositories and DALs as transient services.
- DAL methods normally create and dispose one `MySqlConnection` per operation.
- SQL is embedded in DAL methods. Runtime values are generally passed as Dapper parameters.
- DAL models support query results; repositories commonly reconstruct Domain objects, but not every repository follows one mapping shape.
- Routine reads alone use `IMemoryCache`, with a three-hour entry and explicit invalidation before writes. Routine caching is not a repository-wide policy.
- Pure unit tests verify repository delegation and mapping, routine grouping/cache behavior, DAL model properties, query text transformation, and registration factories without opening MySQL connections.
- A separate Testcontainers.MySql 4.15.0 DAL suite targets `mysql:8.0` with generated credentials and a minimal test-owned schema. **Status: Executed and passing.** 17 tests cover all six DALs, repository behaviors, transaction rollback, isolation reset, and nullable mappings. Infrastructure coverage is 496/599 lines (82.80%) and 19/24 branches (79.17%), exceeding the 60/60 gate.

## Persistence risks

- Repository contracts were narrowed to the operations Application actually uses; unsupported `NotImplementedException` members were removed from `ILogRepository`, `IBodyWeightRepository`, `IExerciseRepository`, and `IRoutineRepository`.
- Routine create/update are executed by `RoutineDAL` inside one connection and transaction with commit/rollback. Rollback behavior has been proven against a real MySQL instance through integration tests that force FK violations mid-transaction.
- `ToReadUncommited()` wraps SQL text with session isolation statements. Integration tests confirm that MySQL's `SET TRANSACTION` (without SESSION) only affects the next single transaction and reverts automatically, so the reset statement is defensive but not strictly required.
- Cancellation tokens received by Application handlers are forwarded through repositories and DALs to Dapper; new I/O paths must keep threading the token.
- The previously dormant Google Sheets integration (packages, source, and `IResourceProvider`/`ResourceProvider` helpers) was removed. MySQL is the sole persistence backend.

Required outcomes are tracked once, in `docs/requirements/technical-requirements.md`.

## Secret remediation and remaining response work

Active values were removed from tracked Web/MAUI configuration, Docker files, and publishing metadata. Local configuration mechanics now avoid committing replacement values. The Google Sheets integration (packages, source, and helpers) was removed entirely; the Google credential resource was removed and the integration is no longer present in the codebase.

Repository cleanup is followed by incident closure: human-confirmed external credential rotation, Google key revocation, Git-history and distributed-copy assessment, and invalidation/replacement of previously produced artifacts have been confirmed completed by the repository owner. Values are intentionally omitted.

Do not retrieve, print, or validate historical values against live services during routine work. The response checklist remains canonical in `docs/requirements/technical-requirements.md`.

## Delivery status

The Windows x64 SDK/workload baseline and project build matrix are documented in `docs/onboarding/getting-started.md`. Web, MAUI Android, and MAUI Windows no-restore builds passed and cover the referenced `net8.0` libraries transitively. These results prove compilation only, not tests, runtime behavior, CI, Docker, publishing, deployment, or rollback. Remaining outcomes are tracked in the canonical gap backlog.

The CI workflow (`.github/workflows/validation.yml`) runs four jobs: unit tests (Domain 100%/100%, Application 80%/80%), integration tests (Infrastructure 60%/60%), secret scanning (gitleaks), and dependency vulnerability scanning. All coverage gates pass; security scans run on every push/PR.

The release workflow (`.github/workflows/release.yml`) builds signed Android APK and publishes Web artifacts on tag push (`v*`). GitHub Release is created automatically with artifacts and release notes. Release and rollback procedures are documented in `docs/infra/release.md`.

The integration suite under `tests/GymTron.IntegrationTests` has been executed against a real MySQL instance via Testcontainers. 17 tests pass, covering all six DALs, repository behaviors, transaction rollback, isolation reset, and nullable mappings. The fixture does not consume Compose configuration, `.env`, application secrets, production credentials, or `init.sql`; all container credentials are unique runtime values.

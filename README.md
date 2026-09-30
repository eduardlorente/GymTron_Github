# GymTron

GymTron is a modern, cross-platform fitness tracking ecosystem built with .NET. It serves as an architectural sandbox to explore and demonstrate Clean Architecture, Domain-Driven Design (DDD), and CQRS across mobile, web, and REST API clients.

While developed as an experimental project rather than a commercial product, it is engineered to production-grade standards: OWASP security hardening, strict automated architecture gates, and comprehensive test coverage (including 100% Domain line and branch coverage).

---

## Key Features

* **Workout & Routine Tracking**: Design custom, multi-day training routines with specific sets, target repetitions, reserve repetitions (RIR), and rest intervals. Record live workout sessions and track completion status.
* **Exercise Catalog & Parameterization**: Rich database of exercises categorizing movement patterns, target muscle groups, and technique execution tips.
* **Body Metrics & Composition**: Log body weight, track Body Mass Index (BMI/IMC), and monitor long-term historical trends.
* **Secure Authentication & Multi-Tenancy**: Built according to OWASP guidelines—JWT authentication, refresh tokens, PBKDF2 password hashing, and user-scoped data isolation (BOLA/IDOR prevention).
* **User & Role Administration**: Administrative Backweb management (`/Users`) allowing administrators (`TypeId = 2`) to view, create, edit, and soft-delete user accounts with role assignments.
* **Cross-Platform Experience**: Mobile client (.NET MAUI for Android & Windows) for in-gym tracking alongside a responsive Web dashboard (ASP.NET Core Razor Pages) for desktop management.
* **Multilingual Experience**: Native localization supporting **Catalan** (default), **Spanish**, and **English**.

---

## Ecosystem Overview

The solution consists of three primary entry points sharing core domain and application libraries:

* **GymTron.App** (.NET 9 MAUI): Cross-platform mobile client targeting Android and Windows x64. Allows users to track workouts, log exercise details, monitor body measurements (weight and BMI), and view historical progress. Consumes the backend exclusively via HTTP through `GymTron.Api`.
* **GymTron.Web** (ASP.NET Core 10 Razor Pages): Web dashboard for routine management, exercise cataloging, and training summaries.
* **GymTron.Api** (ASP.NET Core 10 Minimal API): Secure backend service boundary implementing the REPR (Request-Endpoint-Response) pattern, JWT Bearer authentication, rate limiting, RFC 7807 `ProblemDetails`, and interactive API documentation powered by [Scalar](https://scalar.com).
* **GymTron.Domain & GymTron.Application**: Encapsulate the core business models, domain events, MediatR command/query handlers, FluentValidation pipeline behaviors, and deterministic UTC clock abstractions (`IClock`).
* **GymTron.Infrastructure**: Data access layer built with Dapper and MySQL, featuring transactional atomicity and connection isolation.

---

## Project Structure

```text
GymTron/
├── .github/workflows/          # CI/CD pipelines (validation, security scanning, releases)
├── docs/                       # Architectural documentation, ADRs, and points of truth
├── eng/                        # Build scripts and code coverage assertion gates
├── scripts/                    # Automation and database helper scripts
├── src/
│   ├── GymTron.Api/            # ASP.NET Core 10 Minimal API backend (REPR, JWT, Scalar docs)
│   ├── GymTron.App/            # .NET 9 MAUI cross-platform client (Android & Windows)
│   ├── GymTron.Application/    # CQRS commands/queries (MediatR) and validation behaviors
│   ├── GymTron.Domain/         # Core domain entities, aggregate roots, repository contracts
│   ├── GymTron.Infrastructure/ # Persistence layer (Dapper, MySQL repositories)
│   └── GymTron.Web/            # ASP.NET Core 10 Razor Pages web application
├── tests/
│   ├── GymTron.UnitTests/        # Unit & architecture tests (NetArchTest, 100% Domain coverage)
│   ├── GymTron.IntegrationTests/ # MySQL integration tests via Testcontainers
│   └── GymTron.Web.Tests/        # Web Razor Pages and API client tests
├── docker-compose.yml          # Container configuration for local MySQL database
├── init.sql                    # Database schema creation and initial seed data
└── StartDockerScript.ps1       # Automation script to start the local database container
```

---

## Architectural Principles

* **Clean Architecture & DDD**: Dependencies strictly point inward. Core business logic is encapsulated in `GymTron.Domain` and orchestrated via `GymTron.Application`, independent of external frameworks or databases.
* **CQRS with MediatR**: Commands and queries are cleanly segregated with cross-cutting concerns (validation, logging, exception handling) handled via pipeline behaviors.
* **REPR Pattern & Minimal APIs**: API endpoints are organized around individual request-endpoint-response classes rather than bloated controllers.
* **Automated Architecture Enforcement**: NetArchTest suites in `tests/GymTron.UnitTests/Architecture` enforce layer boundaries, dependency rules, and package restrictions on every build.
* **OWASP Security Baseline**: Client isolation via API, JWT-based authentication, rate limiting on sensitive endpoints, and zero plaintext credentials in source control.

---

## Localization

GymTron provides full multi-language support across the mobile and web clients:

* **Supported Languages**: **Catalan** (default), **Spanish**, and **English**.
* **MAUI**: Resource strings in `src/GymTron.App/Resources/Strings/`, managed via `LocalizationService` and `TranslateExtension` XAML markup.
* **Web**: Resource strings in `src/GymTron.Web/Resources/Pages/`, utilizing `IViewLocalizer` and cookie-based culture persistence.

---

## Getting Started

### Prerequisites

* [.NET 10 SDK](https://dotnet.microsoft.com/download) (build baseline selects `10.0.301` via `global.json`)
* [Docker Desktop](https://www.docker.com/) (for local MySQL instance)
* .NET MAUI Workload (optional, only needed for mobile builds): `android` or `maui-windows`

### 1. Clone the Repository

```bash
git clone https://github.com/eduardlorente/GymTron.git
cd GymTron
```

### 2. Database Setup (Docker)

1. Create a `.env` file in the repository root (see `.env.example` for reference):
   ```env
   MYSQL_ROOT_PASSWORD=YourSecurePasswordHere
   MYSQL_DATABASE=gymtron
   JWT_SECRET_KEY=Your32ByteMinimumSecretKeyHere!
   ```
2. Start the MySQL container:
   ```powershell
   ./StartDockerScript.ps1
   ```
   *Alternatively, run `docker compose up -d`.*

### 3. Application Configuration

Never commit credentials to tracked JSON files. Use ASP.NET Core User Secrets for local development:

* **Web Application**:
  ```powershell
  dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=gymtron;Uid=root;Pwd=YourSecurePasswordHere;" --project src/GymTron.Web/GymTron.Web.csproj
  ```

* **REST API**:
  ```powershell
  dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=gymtron;Uid=root;Pwd=YourSecurePasswordHere;" --project src/GymTron.Api/GymTron.Api.csproj
  dotnet user-secrets set "Jwt:SecretKey" "Your32ByteMinimumSecretKeyHere!" --project src/GymTron.Api/GymTron.Api.csproj
  ```

* **Mobile App (MAUI)**:
  Configure `ApiUrl` in `src/GymTron.App/Resources/Json/appsettings.json` pointing to your local `GymTron.Api` instance.

### 4. Running the Applications

#### REST API (`GymTron.Api`)
```powershell
dotnet run --project src/GymTron.Api/GymTron.Api.csproj
```
* **URL:** `https://localhost:7251` (HTTP: `http://localhost:5275`)
* **Interactive API Documentation (Scalar):** `https://localhost:7251/scalar/v1`

#### Web Dashboard (`GymTron.Web`)
```powershell
dotnet run --project src/GymTron.Web/GymTron.Web.csproj
```
* **URL:** `https://localhost:5000` (HTTP: `http://localhost:5001`)

#### Mobile Client (`GymTron.App` - MAUI)
```powershell
# Windows Desktop
dotnet run --project src/GymTron.App/GymTron.App.csproj -f net9.0-windows10.0.19041.0

# Android (Device or Emulator attached)
dotnet build src/GymTron.App/GymTron.App.csproj -t:Run -f net9.0-android
```

### 5. Default Test Credentials

The database initialization script (`init.sql`) automatically provisions seed accounts for local development and testing:

| Role / Type | Username | Email | Password | Permissions |
|---|---|---|---|---|
| **Standard User** | `user` | `user@gymtron.local` | `password` | Workouts, routines, body weights |
| **Administrator** | `administrator` | `administrator@gymtron.local` | `password` | User Management CRUD (`/Users`) & full platform access |

> [!NOTE]
> Seed routines, workout history, and sample body measurements are linked to test user 1 (`user`) out of the box. Administrative user management (`/Users`) is accessible exclusively by logging in as `administrator`.

---

## Testing & Quality Assurance

The codebase maintains strict automated quality gates:

```powershell
# Run unit and architecture tests
dotnet test tests/GymTron.UnitTests/GymTron.UnitTests.csproj

# Run web frontend tests
dotnet test tests/GymTron.Web.Tests/GymTron.Web.Tests.csproj

# Run MySQL integration tests (requires Docker)
dotnet test tests/GymTron.IntegrationTests/GymTron.IntegrationTests.csproj
```

* **Unit Tests**: Full coverage of Domain logic (enforced at 100% line and branch coverage) and Application handlers (enforced at >= 80% coverage).
* **Architecture Tests**: Automated checks preventing illegal layer references (e.g., UI directly referencing persistence).
* **Web Tests**: Automated tests for Razor Pages models and HTTP API client implementations.
* **Integration Tests**: Tested against real MySQL instances using Testcontainers.
* **Static Analysis**: Roslyn analyzers (`SonarAnalyzer.CSharp`, `Meziantou.Analyzer`) with warnings treated as errors.
* **CI/CD**: GitHub Actions workflows enforce build verification, coverage gates, security scanning (Gitleaks, vulnerable packages), and automated release publishing.

---

## Documentation Hub

Detailed design records, conventions, and operational manuals are maintained in the repository:

* [`POINTS_OF_TRUTH.md`](./POINTS_OF_TRUTH.md) — Index of canonical documentation.
* [`DESIGN.md`](./DESIGN.md) — Current-state architectural charter.
* [`docs/architecture/`](./docs/architecture/) — Dependency maps and architectural guidelines.
* [`docs/decisions/`](./docs/decisions/) — Architecture Decision Records (ADRs).
* [`docs/onboarding/getting-started.md`](./docs/onboarding/getting-started.md) — Contributor guide and verified build matrix.
* [`docs/requirements/technical-requirements.md`](./docs/requirements/technical-requirements.md) — Active capability backlog.

---

## Contributing

1. Fork the repository.
2. Create a feature branch (`git checkout -b feature/my-new-feature`).
3. Commit your changes following [Conventional Commits](https://www.conventionalcommits.org/).
4. Ensure all tests and coverage gates pass (`dotnet test`).
5. Open a Pull Request.

---

## License

This project is licensed under the MIT License — see the [`LICENSE`](./LICENSE) file for details.

## Contact

Eduard Lorente — [eduardlorente@gmail.com](mailto:eduardlorente@gmail.com)
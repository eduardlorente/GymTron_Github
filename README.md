# GymTron

GymTron is a modern, cross-platform fitness tracking ecosystem built with .NET. It serves as an architectural sandbox to explore and demonstrate Clean Architecture, Domain-Driven Design (DDD), and CQRS across mobile, web, and REST API clients.

While developed as an experimental project rather than a commercial product, it is engineered to production-grade standards: OWASP security hardening, strict automated architecture gates, and comprehensive test coverage (including 100% Domain line and branch coverage).

---

## Ecosystem Overview

The solution consists of three primary entry points sharing core domain and application libraries:

* **GymTron.App** (.NET 9 MAUI): Cross-platform mobile client targeting Android and Windows x64. Allows users to track workouts, log exercise details, monitor body measurements (weight and BMI), and view historical progress. Consumes the backend exclusively via HTTP through `GymTron.Api`.
* **GymTron.Web** (ASP.NET Core 10 Razor Pages): Web dashboard for routine management, exercise cataloging, and training summaries.
* **GymTron.Api** (ASP.NET Core 10 Minimal API): Secure backend service boundary implementing the REPR (Request-Endpoint-Response) pattern, JWT Bearer authentication, rate limiting, RFC 7807 `ProblemDetails`, and interactive API documentation powered by [Scalar](https://scalar.com).
* **GymTron.Domain & GymTron.Application**: Encapsulate the core business models, domain events, MediatR command/query handlers, FluentValidation pipeline behaviors, and deterministic UTC clock abstractions (`IClock`).
* **GymTron.Infrastructure**: Data access layer built with Dapper and MySQL, featuring transactional atomicity and connection isolation.

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

---

## Testing & Quality Assurance

The codebase maintains strict automated quality gates:

```powershell
# Run unit and architecture tests
dotnet test tests/GymTron.UnitTests/GymTron.UnitTests.csproj

# Run MySQL integration tests (requires Docker)
dotnet test tests/GymTron.IntegrationTests/GymTron.IntegrationTests.csproj
```

* **Unit Tests**: Full coverage of Domain logic (enforced at 100% line and branch coverage) and Application handlers (enforced at >= 80% coverage).
* **Architecture Tests**: Automated checks preventing illegal layer references (e.g., UI directly referencing persistence).
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
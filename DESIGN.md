# GymTron Architecture Charter

## Current state

GymTron is a multi-project .NET solution with Domain and Application libraries, MySQL/Dapper Infrastructure, and .NET MAUI and Razor Pages composition roots. The former MigratorDB and Tortuga projects are no longer part of the current solution or supported source scope.

The repository follows a clean/CQRS layered architecture with an API service boundary: `GymTron.Domain`, `GymTron.Application`, and `GymTron.Infrastructure` provide core logic and MySQL persistence. `GymTron.Api` acts as the Minimal API service boundary; clients (`GymTron.App` MAUI and `GymTron.Web` Razor Pages) consume the backend via HTTP.

See `docs/architecture/design.md` for the evidence-backed dependency map and entry points.

## Pending

Active technical capability gaps and future enhancements (e.g. automated database migrations and first hosted runner verification) are tracked in `docs/requirements/technical-requirements.md`. Implemented capabilities, security remediations, and architecture enforcement tests are archived in `docs/requirements/implemented-capabilities.md`.

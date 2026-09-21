# ADR-0001: MySQL as sole persistence backend

**Status:** Accepted

## Context

The GymTron codebase contained dormant Google Sheets integration code:
- 3 Google packages referenced in `GymTron.Infrastructure.csproj`: `Google.Apis.Auth`, `Google.Apis.Oauth2.v2`, `Google.Apis.Sheets.v4`
- 6 commented source files in `ExternalServices/GoogleDocuments/` and `Persistence/DAL/GDocs/`
- Unused `IResourceProvider`/`ResourceProvider` helpers registered in DI but only consumed by dormant Google service

The Google Sheets integration was never completed. MySQL is the active, tested, and proven persistence backend with full integration test coverage (17 tests, 82.80% line coverage, 79.17% branch coverage).

## Decision

Remove all Google Sheets integration artifacts:
- Delete 3 Google package references from `GymTron.Infrastructure.csproj`
- Delete 6 dormant source files (`ExternalServices/GoogleDocuments/**`, `Persistence/DAL/GDocs/**`)
- Delete unused `IResourceProvider` (Domain) and `ResourceProvider` (App) helpers
- Remove DI registration from `MauiProgram.cs`

MySQL remains the sole persistence backend.

## Consequences

### Positive
- Simpler architecture: single persistence backend
- Reduced dependency surface: 3 fewer NuGet packages
- Cleaner codebase: no dormant code to confuse contributors
- Faster builds: fewer packages to restore and compile
- Reduced security risk: no unused authentication libraries

### Negative
- If Google Sheets integration is needed in the future, it must be re-implemented from scratch
- Historical context is lost (though preserved in Git history)

### Risks
- **Risk**: Future requirement for Google Sheets integration
  - **Mitigation**: Git history preserves the implementation; can be restored if needed
  - **Likelihood**: Low (project is personal, MySQL is sufficient)

# ADR-0005: CI and Testing Strategy — Discarding Mobile UI Automation and Streamlining CI Gates

**Status:** Accepted

## Context

Prior capability evaluations in `docs/requirements/technical-requirements.md` identified two pending testing and automation gaps:
- **Row 12**: Integration, persistence, frontend, and critical-flow test coverage (noting that MAUI UI and end-to-end user flows were unexecuted in automated suites).
- **Row 14**: Broader CI coverage (noting that the primary CI workflow did not build MAUI or Web, nor run multi-platform execution).

Automated UI testing for cross-platform MAUI applications requires emulators/simulators, specialized platform runners (macOS/Windows), and heavy UI automation drivers (e.g. Appium). In practice, mobile UI automation suffers from high maintenance overhead, execution flakiness, and disproportionate CI compute costs relative to value delivered.

Furthermore, following the architectural decoupling of `GymTron.App` into an HTTP client consuming `GymTron.Api`, the presentation layer is purely declarative (XAML with compiled bindings `x:DataType`) and view-model driven. All core business rules, validation, domain invariants, and persistence transactions live in `GymTron.Domain`, `GymTron.Application`, and `GymTron.Insfrastructure`.

## Decision

1. **Discard Automated Mobile UI / E2E Testing**:
   - Formally discard automated UI and end-to-end testing for `GymTron.App`.
   - UI reliability is instead guaranteed through:
     - Compile-time checking via XAML compiled bindings (`x:DataType`).
     - Decoupled, strongly-typed API models and contracts (`IGymTronApiClient`).
     - Unit testing of Domain and Application logic.
     - Manual smoke testing prior to production APK generation.

2. **Streamlined, Fast CI Gates**:
   - The primary CI pipeline (`.github/workflows/validation.yml`) remains strictly focused on headless, high-signal, reproducible gates running on lightweight Linux runners (`ubuntu-latest`):
     - **Security scanning**: Secret scanning (`gitleaks`) and dependency vulnerability auditing.
     - **Static analysis**: Compiler zero-warning gate (`TreatWarningsAsErrors`) and analyzer enforcement (`SonarAnalyzer`, `Meziantou`).
     - **Architecture enforcement**: NetArchTest boundary validation (`ArchitectureTests.cs`).
     - **Unit testing & Coverage**: 100% line/branch gate for `GymTron.Domain` and 80% line/branch gate for `GymTron.Application`.
     - **Persistence integration testing**: Real MySQL database testing using service containers (enforcing 60% coverage gate for `GymTron.Insfrastructure`).
   - MAUI workload restoration and APK compilation are restricted solely to the dedicated on-demand release workflow (`release.yml` triggered by tags `v*`), eliminating MAUI overhead from daily PR/commit validation.

3. **Backlog Pruning**:
   - Mark Rows 12 and 14 of `docs/requirements/technical-requirements.md` as Resolved/Discarded by this ADR.
   - Mark Row 17 (MAUI direct database access) as Implemented following the introduction of `GymTron.Api` and complete HTTP client decoupling.

## Consequences

### Positive
- Prevents CI maintenance burnout, eliminating brittle emulator startups and flaky UI test failures.
- Keeps PR feedback fast (runs complete in minutes on standard Linux runners).
- Centers testing effort on high-ROI layers: Domain invariants, Application use cases, API contracts, and real SQL persistence behavior.
- Eliminates CI cost and time waste by building MAUI only during intentional release cycles.

### Negative
- Visual layout regressions, rendering anomalies, or platform-specific OS view glitches are not caught automatically in CI; they require human smoke testing on real devices.

### Risks
- **Risk**: A breaking change in API payload shapes goes unnoticed by the MAUI app.
  - **Mitigation**: Shared or aligned DTO models and contract-level integration/unit tests ensure serialization contracts remain backwards-compatible.

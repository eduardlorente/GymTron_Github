# ADR-0002: Accept `Insfrastructure` typo as permanent

**Status:** Superseded by Architecture Modernization (Phase 3)

## Note
Superseded during the architectural audit remediation (Phase 3). The assembly, folder, namespace, CI, tests, and configuration were systematically renamed to `GymTron.Infrastructure`.

## Context

The project contains a typo in the Infrastructure layer:
- Project name: `GymTron.Insfrastructure` (should be `Infrastructure`)
- Namespace: `GymTron.Insfrastructure.*`
- Directory: `src/GymTron.Insfrastructure/`
- Referenced in 5+ project files, CI workflows, documentation, and solution file

The typo is pervasive and has been present since the project's early history. Renaming would require:
- Renaming the project directory and `.csproj` file
- Updating all `using` statements across the codebase
- Updating all project references
- Updating CI workflows
- Updating documentation
- Updating solution file
- Risk of breaking external tooling or integrations

## Decision

Accept `Insfrastructure` as the permanent spelling. Do not rename.

Document this decision to prevent future contributors from attempting to "fix" the typo without understanding the impact.

## Consequences

### Positive
- Avoids risky refactor that touches 50+ files
- No risk of breaking external tooling or integrations
- No merge conflicts with other branches
- Contributors are not distracted by the typo

### Negative
- The typo remains visible in the codebase
- May confuse new contributors who notice the typo
- Inconsistent with standard English spelling

### Risks
- **Risk**: New contributors attempt to "fix" the typo without understanding the impact
  - **Mitigation**: This ADR documents the decision; add a note in `docs/architecture/design.md`
  - **Likelihood**: Low (most contributors will notice the ADR)

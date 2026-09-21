# Documentation Hub

Use this directory for durable technical guidance. Root files remain short entry points; each topic has one canonical home here.

| Hub | Owns | Does not own |
|---|---|---|
| `architecture/` | Current module boundaries, dependencies, entry points, and rule precedence | Feature requirements or language syntax |
| `language/` | Repository-approved language conventions | Architecture or delivery procedures |
| `frontend/` | Current MAUI and Web presentation patterns, inconsistencies, and boundaries | Cross-layer architecture or pending capability ownership |
| `infra/` | Configuration, persistence, integrations, and delivery evidence | Secret values or product behavior |
| `requirements/` | Pending technical capability outcomes | Claims that missing capabilities already exist |
| `onboarding/` | Safe read order and command classification | Product README content |
| `testing/` | Canonical automated-test scope, structure, commands, and coverage boundaries | Claims of integration, frontend, or end-to-end coverage |
| `api/` | Minimal API conventions, REPR endpoint patterns, route rules, and RFC 7807 error handling | Application handler logic or persistence operations |
| `decisions/` | Future durable ADR lifecycle and registry | Fictional or feature-local decisions |

Start at `POINTS_OF_TRUTH.md`. Link to canonical guidance instead of copying it.

# Architecture Decision Records

**Status: Implemented**

ADRs record approved, durable, cross-cutting decisions and their rationale. Feature-local choices belong with the feature/change design. Superseded ADRs remain in history with an explicit status.

## Process

### Lifecycle

1. **Proposed**: Draft ADR in PR, discussion in comments
2. **Accepted**: Merge to main, decision becomes official
3. **Deprecated**: No longer applies, maintained for historical context
4. **Superseded**: Replaced by another ADR (reference the new ADR)

### Numbering

- Format: `ADR-NNNN` (4 digits, zero-padded)
- Sequential: ADR-0001, ADR-0002, etc.
- No gaps: Use the next available number

### File naming

- Location: `docs/decisions/ADR-NNNN-title.md`
- Example: `docs/decisions/ADR-0001-mysql-persistence.md`
- Use lowercase, hyphens for spaces

### Review authority

- Repository owner approves ADRs
- Discussion happens in PR comments
- Merge when consensus is reached

### Template

Use `docs/decisions/TEMPLATE.md` for new ADRs.

## Registry

| ADR | Title | Status | Date |
|-----|-------|--------|------|
| ADR-0001 | MySQL as sole persistence backend | Accepted | 2026-09-09 |
| ADR-0002 | Accept `Insfrastructure` typo as permanent | Superseded | 2026-09-09 |
| ADR-0003 | FluentValidation pipeline for request validation | Accepted | 2026-09-09 |
| ADR-0004 | Architecture and dependency enforcement | Accepted | 2026-09-15 |
| ADR-0005 | CI and Testing Strategy — Discarding Mobile UI Automation and Streamlining CI Gates | Accepted | 2026-09-15 |

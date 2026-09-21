# GymTron Agent Policy

This file is the operational entry point for automated contributors. Detailed rules live in the focused documents indexed by `POINTS_OF_TRUTH.md`.

## Required workflow

1. Read `POINTS_OF_TRUTH.md` first, then this file.
2. Load only the focused documentation relevant to the task.
3. Inspect current code and configuration before relying on a claim that is pending, disputed, or potentially stale.
4. Preserve unrelated work. Do not modify product code, dependencies, CI, runtime configuration, solution metadata, or deployment assets without explicit task approval.
5. Use only commands already verified for the intended purpose. If verification is unavailable, report the limitation and the corresponding gap in `docs/requirements/technical-requirements.md`.
6. Report discovered conflicts, uncertainty, and missing capabilities rather than inventing policy.
7. Update the focused canonical document and `POINTS_OF_TRUTH.md` whenever durable guidance or navigation changes.

## Language policy

- Code identifiers, logs, and technical exception messages are written in English.
- Technical documentation is written in English.
- Detailed C# guidance is canonical in `docs/language/csharp.md`.

## Security policy (OWASP compliance)

All new implementations, endpoints, and refactorings MUST adhere to OWASP security principles (OWASP Top 10):

1. **Access Control & Multi-Tenancy (BOLA/IDOR)**: Enforce user data isolation. Commands, queries, and repositories must scope private data access to the authenticated user ID (`ClaimsPrincipal.GetUserId()`) unless the resource is explicitly public or system-wide. Verify ownership before executing mutations or updates.
2. **Authentication & Route Protection**: Protect all business endpoints and web routes by default. Require authenticated users via fallback policies or folder authorization conventions.
3. **Abuse Prevention & Rate Limiting**: Apply rate limiting on authentication and sensitive public endpoints to prevent credential stuffing and brute-force attacks.
4. **Cryptographic & Secret Security**: Never commit or fallback to hardcoded secrets or connection strings. Enforce cryptographic key length requirements (e.g. JWT secret >= 32 bytes / 256 bits).
5. **Transport & Header Hardening**: Enforce HTTPS/HSTS, secure cookie configurations (`HttpOnly`, `SameSite=Lax`, `SecurePolicy=Always`), and defense-in-depth HTTP security headers (`X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy: strict-origin-when-cross-origin`).
6. **Input Validation & Safe Handling**: Validate all command and query inputs using FluentValidation before execution. Never catch and silently swallow exceptions; preserve full diagnostics while avoiding sensitive leakages in public error responses (using RFC 7807 `ProblemDetails`).

## Change gate

Requirements, current implementation evidence, and applicable focused guidance must agree before implementation. Escalate unresolved conflicts as a specific decision; do not silently convert an aspiration into a current rule.

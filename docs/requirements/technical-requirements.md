# Technical Capability Requirements

All entries below represent genuine, pending capability gaps and technical enhancements. Completed capabilities and remediations are archived in `docs/requirements/implemented-capabilities.md`.

| Gap | Evidence | Impact | Required outcome | Priority | Status | Owner/decision needed |
|---|---|---|---|---|---|---|
| Automated database migration system | Currently, schema initialization relies on `init.sql` and manual updates; no automated migration tool (e.g., DbUp, FluentMigrator) is integrated into runtime or deployment pipelines. | Schema modifications across environments require manual SQL execution, creating risk during deployments or rollbacks. | Introduce a versioned, automated migration tool that executes predictably during startup or CI/CD deployment. | Medium | Pending | Architecture and Database owners determine migration tooling. |
| Hosted CI/CD run verification | GitHub Actions workflows (`validation.yml` and `release.yml`) are configured, but first runs on hosted `ubuntu-latest` / GitHub runners are pending until pushed to the remote repository. | Remote runner dependencies, secrets, and timing are unverified until the first hosted execution. | Verify the complete suite execution and artifact publication on GitHub Actions upon remote push. | Medium | Pending (awaiting remote execution) | Repository owner reviews first hosted run. |

# ADR-0003: FluentValidation pipeline for request validation

**Status:** Accepted

## Context

The Application layer had inconsistent validation:
- Some handlers manually instantiated validators and called `ValidateAndThrow()`
- Other handlers had no validation at all
- Validation logic was scattered across handlers
- No centralized validation enforcement

The codebase already used FluentValidation for some commands/queries, but there was no consistent pattern for how validation was applied.

## Decision

Implement a MediatR pipeline behavior (`ValidationBehavior<TRequest, TResponse>`) that:
1. Resolves all registered `IValidator<TRequest>` instances from DI
2. Executes validation before the handler runs
3. Throws `ValidationException` if validation fails
4. Allows handlers to assume validated input

Register validators via `services.AddValidatorsFromAssemblyContaining<StartTrainingCommand>()` in `ServiceCollectionExtensions.AddApplicationServices()`.

Remove manual `ValidateAndThrow()` calls from handlers (6 handlers updated).

## Consequences

### Positive
- Centralized validation: all requests validated consistently
- Separation of concerns: handlers focus on business logic, not validation
- Easier to add new validators: just register them, pipeline handles execution
- Better testability: validators can be tested independently
- Consistent error handling: `ValidationException` thrown uniformly

### Negative
- Slight performance overhead: pipeline adds one more step to request processing
- Validators must be registered in DI (additional configuration)
- Breaking change: handlers that relied on manual validation must be updated

### Risks
- **Risk**: Validators not registered in DI, so validation is skipped
  - **Mitigation**: Document validator registration pattern; add test to verify all validators are registered
  - **Likelihood**: Low (registration is centralized in one place)
- **Risk**: Pipeline behavior order matters (validation must run before handler)
  - **Mitigation**: Document pipeline behavior registration order; test pipeline behavior
  - **Likelihood**: Low (MediatR executes pipeline behaviors in registration order)

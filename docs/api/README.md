# API Architecture and Rules

This guide defines canonical patterns, conventions, and operational boundaries for `src/GymTron.Api`.

## Architecture and Scope

`GymTron.Api` is an ASP.NET Core Minimal API targeting `.NET 10.0`. It acts as the backend service boundary for mobile clients (`GymTron.App`) and decoupled consumers.

Key boundaries:
- **Zero direct persistence access**: Endpoints dispatch exclusively through MediatR commands and queries defined in `GymTron.Application`. Endpoints must never directly reference Infrastructure repositories, DALs, or Dapper.
- **Composition Root**: Configures DI for Application and Infrastructure services, OpenAPI, exception handling, and endpoint routing.
- **Stateless HTTP**: Endpoints are stateless and communicate via standard HTTP verbs and JSON payloads.

## Request-Endpoint-Response (REPR) Pattern

Endpoints follow the Request-Endpoint-Response (REPR) pattern:

1. **Organization**: Grouped by feature domain under `Endpoints/<Feature>/` (e.g., `Endpoints/Trainings/`, `Endpoints/Routines/`).
2. **Single Responsibility**: Each endpoint file encapsulates its specific request contract (if any) and route mapping (e.g., `StartTrainingEndpoint.cs`).
3. **Route Mapping Extensions**: Each endpoint defines an extension method on `IEndpointRouteBuilder` (e.g., `public static void MapStartTrainingEndpoint(this IEndpointRouteBuilder app)`).
4. **Central Registration**: All endpoint mappers are aggregated in `Endpoints/EndpointExtensions.cs` via `MapApiEndpoints(this IEndpointRouteBuilder app)`.

### Endpoint Example

```csharp
namespace GymTron.Api.Endpoints.Trainings;

public record StartTrainingRequest(int RoutineId, int DayOfWeek);

public static class StartTrainingEndpoint
{
    public static void MapStartTrainingEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/trainings/start", async (StartTrainingRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var command = new StartTrainingCommand(Guid.NewGuid(), request.RoutineId, request.DayOfWeek);
            await mediator.Send(command, ct);
            return Results.Ok();
        })
        .WithTags("Trainings")
        .WithName("StartTraining")
        .WithSummary("Start a new training session");
    }
}
```

## Route and OpenAPI Conventions

- **Prefix**: All API routes begin with `/api/` followed by the resource in plural form (e.g., `/api/routines`, `/api/trainings`, `/api/bodyweights`).
- **HTTP Verbs**:
  - `GET`: Read resources without side-effects. Returns `200 OK` or `404 Not Found`.
  - `POST`: Create a resource or execute an action. Returns `200 OK`, `201 Created`, or `204 NoContent`.
  - `PUT`: Full update of a resource.
  - `DELETE`: Remove a resource. Returns `200 OK` or `204 NoContent`.
- **OpenAPI Metadata**: Every endpoint must declare:
  - `.WithTags("<Feature>")`: Groups related endpoints in OpenAPI/Swagger documentation.
  - `.WithName("<ActionName>")`: Unique identifier for client SDK generation.
  - `.WithSummary("<ShortDescription>")`: Human-readable summary of the operation.
- **OpenAPI and Interactive Documentation**:
  - Technical OpenAPI v3 specification is exposed at `/openapi/v1.json` in development.
  - Interactive API documentation and explorer is provided via Scalar at `/scalar/v1` in development.
  - Both endpoints allow anonymous access in development mode to simplify local testing.

## Error Handling and RFC 7807 ProblemDetails

All unhandled exceptions in the request pipeline are captured by `GlobalExceptionHandler` (`IExceptionHandler`) and formatted as RFC 7807 `ProblemDetails`:

| Exception Type | HTTP Status | ProblemDetails Type | Description |
|---|---|---|---|
| `ValidationException` (FluentValidation) | `400 Bad Request` | `ValidationProblemDetails` | Contains field-level validation errors grouped by property name. |
| `EntityNotFoundException` | `404 Not Found` | `ProblemDetails` | Resource not found in domain or storage. |
| `DomainException` / `InvalidDomainOperationException` | `400 Bad Request` | `ProblemDetails` | Business rule or domain state violation. |
| Unhandled / System `Exception` | `500 Internal Server Error` | `ProblemDetails` | Unexpected system failure; logs full error details while hiding sensitive internals. |

Exceptions must not be caught and swallowed in endpoint lambda delegates; allow them to bubble up to `GlobalExceptionHandler` to ensure uniform status codes and responses.

## Asynchronous Flow and Cancellation

- All endpoint delegates must accept a `CancellationToken ct` parameter and forward it to `mediator.Send(..., ct)`.
- Never use `.Result` or `.Wait()`; always `await` asynchronous calls.

## DTOs and Data Mapping

- Input request DTOs should be declared as immutable C# `record` types within the endpoint file or a shared `DTO` folder if reused across endpoints of the same feature.
- Use explicit mapping extension methods (e.g., `TrainingMappingExtensions.cs`) to project Application query results into API responses. Keep mapping pure and deterministic.

## Security and Multi-Tenancy Rules (OWASP Compliance)

Every new endpoint or refactoring must enforce OWASP Top 10 security baselines:

1. **Authentication by Default**: The API uses a fallback authorization policy requiring an authenticated user (`RequireAuthenticatedUser`). Only explicitly anonymous endpoints (e.g. `/api/auth/login`, `/api/auth/register`) may use `.AllowAnonymous()`.
2. **Multi-Tenant Isolation (BOLA/IDOR Prevention)**:
   - Endpoints must inject `ClaimsPrincipal user` and extract the authenticated user identifier via `user.GetUserId()`.
   - Forward `userId` to the corresponding Application command or query.
   - Handlers and repositories must filter and scope private entities strictly to `userId`. Cross-user access or mutations on foreign resources must throw `EntityNotFoundException` or yield `404 Not Found`.
3. **Abuse Prevention & Rate Limiting**:
   - Authentication and sensitive public endpoints must declare `.RequireRateLimiting(RateLimitingConstants.AuthPolicy)` to protect against brute-force and credential stuffing.
4. **Input Validation**:
   - All input contracts must be validated via FluentValidation before handler execution.
5. **Safe Error Handling**:
   - Let unhandled exceptions bubble to `GlobalExceptionHandler` so they format as RFC 7807 `ProblemDetails` without leaking stack traces or internal implementation details.


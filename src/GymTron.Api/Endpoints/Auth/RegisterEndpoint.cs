using GymTron.Api.Infrastructure;
using GymTron.Application.Auth.Commands.Register;
using MediatR;

namespace GymTron.Api.Endpoints.Auth;

public record RegisterRequest(string Username, string Email, string Password);

public static class RegisterEndpoint
{
    public static void MapRegisterEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/register", async (RegisterRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var command = new RegisterUserCommand(Guid.NewGuid(), request.Username, request.Email, request.Password);
            int userId = await mediator.Send(command, ct);
            return Results.Created($"/api/users/{userId}", new { UserId = userId });
        })
        .WithTags("Auth")
        .WithName("Register")
        .WithSummary("Register a new user account")
        .RequireRateLimiting(RateLimitingConstants.AuthPolicy)
        .AllowAnonymous();
    }
}

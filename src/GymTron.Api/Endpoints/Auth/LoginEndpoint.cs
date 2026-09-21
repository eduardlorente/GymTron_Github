using GymTron.Api.Infrastructure;
using GymTron.Application.Auth.Commands.Login;
using GymTron.Application.Auth.Models;
using MediatR;

namespace GymTron.Api.Endpoints.Auth;

public record LoginRequest(string Identifier, string Password);

public static class LoginEndpoint
{
    public static void MapLoginEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", async (LoginRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var command = new LoginCommand(Guid.NewGuid(), request.Identifier, request.Password);
            AuthResult result = await mediator.Send(command, ct);
            return Results.Ok(result);
        })
        .WithTags("Auth")
        .WithName("Login")
        .WithSummary("Authenticate with username or email and password")
        .RequireRateLimiting(RateLimitingConstants.AuthPolicy)
        .AllowAnonymous();
    }
}

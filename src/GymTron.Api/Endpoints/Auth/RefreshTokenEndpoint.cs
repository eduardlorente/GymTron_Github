using GymTron.Api.Infrastructure;
using GymTron.Application.Auth.Commands.Refresh;
using GymTron.Application.Auth.Models;
using MediatR;

namespace GymTron.Api.Endpoints.Auth;

public record RefreshTokenRequest(string RefreshToken);

public static class RefreshTokenEndpoint
{
    public static void MapRefreshTokenEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/refresh", async (RefreshTokenRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var command = new RefreshTokenCommand(Guid.NewGuid(), request.RefreshToken);
            AuthResult result = await mediator.Send(command, ct);
            return Results.Ok(result);
        })
        .WithTags("Auth")
        .WithName("RefreshToken")
        .WithSummary("Refresh access token using an existing refresh token")
        .RequireRateLimiting(RateLimitingConstants.AuthPolicy)
        .AllowAnonymous();
    }
}

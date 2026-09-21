using GymTron.Api.Infrastructure;
using GymTron.Application.Auth.Commands.Revoke;
using MediatR;

namespace GymTron.Api.Endpoints.Auth;

public record RevokeTokenRequest(string RefreshToken);

public static class RevokeEndpoint
{
    public static void MapRevokeEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/revoke", async (RevokeTokenRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var command = new RevokeTokenCommand(Guid.NewGuid(), request.RefreshToken);
            await mediator.Send(command, ct);
            return Results.NoContent();
        })
        .WithTags("Auth")
        .WithName("RevokeToken")
        .WithSummary("Revoke a refresh token on logout")
        .RequireRateLimiting(RateLimitingConstants.AuthPolicy);
    }
}

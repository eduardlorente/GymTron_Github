using GymTron.Application.Users.Queries;
using MediatR;

namespace GymTron.Api.Endpoints.Users;

public static class ListUsersEndpoint
{
    public static void MapListUsersEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/users", async (IMediator mediator, CancellationToken ct) =>
        {
            var users = await mediator.Send(new ListUsersQuery(Guid.NewGuid()), ct);
            return Results.Ok(users);
        })
        .WithTags("Users")
        .WithName("ListUsers")
        .WithSummary("List all users (Admin only)")
        .RequireAuthorization(policy => policy.RequireRole("Administrator"));
    }
}

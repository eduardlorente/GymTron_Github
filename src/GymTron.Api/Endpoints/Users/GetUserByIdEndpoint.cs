using GymTron.Application.Users.Queries;
using MediatR;

namespace GymTron.Api.Endpoints.Users;

public static class GetUserByIdEndpoint
{
    public static void MapGetUserByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/users/{id:int}", async (int id, IMediator mediator, CancellationToken ct) =>
        {
            var user = await mediator.Send(new GetUserByIdQuery(Guid.NewGuid(), id), ct);
            return user == null ? Results.NotFound() : Results.Ok(user);
        })
        .WithTags("Users")
        .WithName("GetUserById")
        .WithSummary("Get user details by ID (Admin only)")
        .RequireAuthorization(policy => policy.RequireRole("Administrator"));
    }
}

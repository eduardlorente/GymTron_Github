using System.Security.Claims;
using GymTron.Api.Infrastructure;
using GymTron.Application.Routines.Queries;
using MediatR;

namespace GymTron.Api.Endpoints.Routines;

public static class ListAllRoutinesEndpoint
{
    public static void MapListAllRoutinesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/routines", async (ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            var routines = await mediator.Send(new ListAllQuery(Guid.NewGuid(), user.GetUserId()), ct);
            return Results.Ok(routines);
        })
        .WithTags("Routines")
        .WithName("ListAllRoutines")
        .WithSummary("List all routines");
    }
}

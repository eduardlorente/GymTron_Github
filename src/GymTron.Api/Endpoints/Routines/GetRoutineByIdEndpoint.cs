using System.Security.Claims;
using GymTron.Api.Infrastructure;
using GymTron.Application.Routines.Queries;
using MediatR;

namespace GymTron.Api.Endpoints.Routines;

public static class GetRoutineByIdEndpoint
{
    public static void MapGetRoutineByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/routines/{id:int}", async (int id, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            var routine = await mediator.Send(new GetRoutineByIdQuery(Guid.NewGuid(), id, user.GetUserId()), ct);
            return routine is not null ? Results.Ok(routine) : Results.NotFound();
        })
        .WithTags("Routines")
        .WithName("GetRoutineById")
        .WithSummary("Get routine by ID");
    }
}

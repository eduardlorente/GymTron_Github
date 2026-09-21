using System.Security.Claims;
using GymTron.Api.Infrastructure;
using GymTron.Application.Routines.Commands;
using MediatR;

namespace GymTron.Api.Endpoints.Routines;

public record CreateRoutineRequest(string Name, List<RoutineItemInput> Items);

public static class CreateRoutineEndpoint
{
    public static void MapCreateRoutineEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/routines", async (CreateRoutineRequest request, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            var command = new CreateRoutineCommand(Guid.NewGuid(), request.Name, request.Items, user.GetUserId());
            var id = await mediator.Send(command, ct);
            return Results.Created($"/api/routines/{id}", new { id });
        })
        .WithTags("Routines")
        .WithName("CreateRoutine")
        .WithSummary("Create a new routine");
    }
}

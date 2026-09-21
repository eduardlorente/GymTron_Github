using System.Security.Claims;
using GymTron.Api.Infrastructure;
using GymTron.Application.Routines.Commands;
using MediatR;

namespace GymTron.Api.Endpoints.Routines;

public record UpdateRoutineRequest(string Name, List<RoutineItemInput> Items);

public static class UpdateRoutineEndpoint
{
    public static void MapUpdateRoutineEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/routines/{id:int}", async (int id, UpdateRoutineRequest request, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            var command = new UpdateRoutineCommand(Guid.NewGuid(), id, request.Name, request.Items, user.GetUserId());
            await mediator.Send(command, ct);
            return Results.NoContent();
        })
        .WithTags("Routines")
        .WithName("UpdateRoutine")
        .WithSummary("Update an existing routine");
    }
}

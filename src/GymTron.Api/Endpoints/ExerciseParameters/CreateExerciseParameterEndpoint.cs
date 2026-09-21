using GymTron.Application.ExerciseParameters.Commands;
using GymTron.Domain.Enums;
using MediatR;

namespace GymTron.Api.Endpoints.ExerciseParameters;

public record CreateExerciseParameterRequest(string Name, string Description, string Pattern, ExerciseTypes Type, int? ReplaysInReserve);

public static class CreateExerciseParameterEndpoint
{
    public static void MapCreateExerciseParameterEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/exercise-parameters", async (CreateExerciseParameterRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var command = new CreateExerciseParameterCommand(
                Guid.NewGuid(),
                request.Name,
                request.Description,
                request.Pattern,
                request.Type,
                request.ReplaysInReserve);
            var id = await mediator.Send(command, ct);
            return Results.Created($"/api/exercise-parameters/{id}", new { id });
        })
        .WithTags("ExerciseParameters")
        .WithName("CreateExerciseParameter")
        .WithSummary("Create a new exercise parameter");
    }
}

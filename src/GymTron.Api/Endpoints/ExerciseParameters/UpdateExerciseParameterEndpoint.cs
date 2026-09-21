using GymTron.Application.ExerciseParameters.Commands;
using GymTron.Domain.Enums;
using MediatR;

namespace GymTron.Api.Endpoints.ExerciseParameters;

public record UpdateExerciseParameterRequest(string Name, string Description, string Pattern, ExerciseTypes Type, int? ReplaysInReserve);

public static class UpdateExerciseParameterEndpoint
{
    public static void MapUpdateExerciseParameterEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/exercise-parameters/{id:int}", async (int id, UpdateExerciseParameterRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var command = new UpdateExerciseParameterCommand(
                Guid.NewGuid(),
                id,
                request.Name,
                request.Description,
                request.Pattern,
                request.Type,
                request.ReplaysInReserve);
            await mediator.Send(command, ct);
            return Results.NoContent();
        })
        .WithTags("ExerciseParameters")
        .WithName("UpdateExerciseParameter")
        .WithSummary("Update an existing exercise parameter");
    }
}

using GymTron.Application.ExerciseParameters.Queries;
using MediatR;

namespace GymTron.Api.Endpoints.ExerciseParameters;

public static class GetExerciseParameterByIdEndpoint
{
    public static void MapGetExerciseParameterByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/exercise-parameters/{id:int}", async (int id, IMediator mediator, CancellationToken ct) =>
        {
            var exercise = await mediator.Send(new GetExerciseParameterByIdQuery(Guid.NewGuid(), id), ct);
            return exercise is not null ? Results.Ok(exercise) : Results.NotFound();
        })
        .WithTags("ExerciseParameters")
        .WithName("GetExerciseParameterById")
        .WithSummary("Get exercise parameter by ID");
    }
}

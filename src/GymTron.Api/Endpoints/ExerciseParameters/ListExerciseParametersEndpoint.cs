using GymTron.Application.ExerciseParameters.Queries;
using MediatR;

namespace GymTron.Api.Endpoints.ExerciseParameters;

public static class ListExerciseParametersEndpoint
{
    public static void MapListExerciseParametersEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/exercise-parameters", async (IMediator mediator, CancellationToken ct) =>
        {
            var exercises = await mediator.Send(new ListExerciseParametersQuery(Guid.NewGuid()), ct);
            return Results.Ok(exercises);
        })
        .WithTags("ExerciseParameters")
        .WithName("ListExerciseParameters")
        .WithSummary("List all exercise parameters");
    }
}

using System.Security.Claims;
using GymTron.Api.Infrastructure;
using GymTron.Application.Exercises.Queries;
using MediatR;

namespace GymTron.Api.Endpoints.Exercises;

public static class GetExerciseHistoryEndpoint
{
    public static void MapGetExerciseHistoryEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/exercises/history", async (ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            var exercises = await mediator.Send(new ExerciseHistoryQuery(Guid.NewGuid(), user.GetUserId()), ct);
            return Results.Ok(exercises);
        })
        .WithTags("Exercises")
        .WithName("GetExerciseHistory")
        .WithSummary("Get exercise history");
    }
}

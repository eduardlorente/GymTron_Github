using System.Security.Claims;
using GymTron.Api.Infrastructure;
using GymTron.Application.Trainings.Commands;
using GymTron.Application.Trainings.Queries.DTO;
using MediatR;

namespace GymTron.Api.Endpoints.Trainings;

public record AddExerciseToTrainingRequest(
    TrainingDto CurrentTraining,
    int ExerciseParametersId,
    string ExerciseParametersName,
    decimal? Weight,
    int? Repetitions,
    int? DurationInSeconds,
    List<string> Observations);

public static class AddExerciseToTrainingEndpoint
{
    public static void MapAddExerciseToTrainingEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/trainings/exercises", async (AddExerciseToTrainingRequest request, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            var trainingDomain = request.CurrentTraining.ToDomain(user.GetUserId());
            AddExerciseToTrainingCommand command = request.DurationInSeconds.HasValue
                ? AddExerciseToTrainingCommand.New(
                    Guid.NewGuid(),
                    trainingDomain,
                    request.ExerciseParametersId,
                    request.ExerciseParametersName,
                    request.DurationInSeconds,
                    request.Observations)
                : AddExerciseToTrainingCommand.New(
                    Guid.NewGuid(),
                    trainingDomain,
                    request.ExerciseParametersId,
                    request.ExerciseParametersName,
                    request.Weight,
                    request.Repetitions,
                    request.Observations);

            var updatedTraining = await mediator.Send(command, ct);
            return Results.Ok(updatedTraining.ToDto());
        })
        .WithTags("Trainings")
        .WithName("AddExerciseToTraining")
        .WithSummary("Add exercise to active training session");
    }
}

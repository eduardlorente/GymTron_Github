using System.Security.Claims;
using GymTron.Api.Infrastructure;
using GymTron.Application.Trainings.Commands;
using GymTron.Application.Trainings.Queries.DTO;
using MediatR;

namespace GymTron.Api.Endpoints.Trainings;

public record FinishTrainingRequest(TrainingDto Training);

public static class FinishTrainingEndpoint
{
    public static void MapFinishTrainingEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/trainings/finish", async (FinishTrainingRequest request, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            var command = new FinishTrainingCommand(Guid.NewGuid(), request.Training.ToDomain(user.GetUserId()));
            await mediator.Send(command, ct);
            return Results.Ok();
        })
        .WithTags("Trainings")
        .WithName("FinishTraining")
        .WithSummary("Finish active training session");
    }
}

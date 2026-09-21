using System.Security.Claims;
using GymTron.Api.Infrastructure;
using GymTron.Application.Trainings.Commands;
using GymTron.Application.Trainings.Queries.DTO;
using MediatR;

namespace GymTron.Api.Endpoints.Trainings;

public record CancelTrainingRequest(TrainingDto Training);

public static class CancelTrainingEndpoint
{
    public static void MapCancelTrainingEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/trainings/cancel", async (CancelTrainingRequest request, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            var command = new CancelTrainingCommand(Guid.NewGuid(), request.Training.ToDomain(user.GetUserId()));
            await mediator.Send(command, ct);
            return Results.Ok();
        })
        .WithTags("Trainings")
        .WithName("CancelTraining")
        .WithSummary("Cancel active training session");
    }
}

using System.Security.Claims;
using GymTron.Api.Infrastructure;
using GymTron.Application.Trainings.Commands;
using MediatR;

namespace GymTron.Api.Endpoints.Trainings;

public record StartTrainingRequest(int RoutineId, int DayOfWeek);

public static class StartTrainingEndpoint
{
    public static void MapStartTrainingEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/trainings/start", async (StartTrainingRequest request, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            var command = new StartTrainingCommand(Guid.NewGuid(), request.RoutineId, request.DayOfWeek, user.GetUserId());
            await mediator.Send(command, ct);
            return Results.Ok();
        })
        .WithTags("Trainings")
        .WithName("StartTraining")
        .WithSummary("Start a new training session");
    }
}

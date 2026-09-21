using System.Security.Claims;
using GymTron.Api.Infrastructure;
using GymTron.Application.Trainings.Queries;
using MediatR;

namespace GymTron.Api.Endpoints.Trainings;

public static class GetCurrentTrainingEndpoint
{
    public static void MapGetCurrentTrainingEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/trainings/current", async (ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            var training = await mediator.Send(new CurrentTrainingQuery(Guid.NewGuid(), user.GetUserId()), ct);
            return Results.Ok(training);
        })
        .WithTags("Trainings")
        .WithName("GetCurrentTraining")
        .WithSummary("Get the current active training session");
    }
}

using System.Security.Claims;
using GymTron.Api.Infrastructure;
using GymTron.Application.Trainings.Queries;
using MediatR;

namespace GymTron.Api.Endpoints.Trainings;

public static class GetTrainingHistoryEndpoint
{
    public static void MapGetTrainingHistoryEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/trainings/history", async (ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            var history = await mediator.Send(new HistoryTrainingsQuery(Guid.NewGuid(), user.GetUserId()), ct);
            return Results.Ok(history);
        })
        .WithTags("Trainings")
        .WithName("GetTrainingHistory")
        .WithSummary("Get training history");
    }
}

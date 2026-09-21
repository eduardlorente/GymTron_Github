using System.Security.Claims;
using GymTron.Api.Infrastructure;
using GymTron.Application.BodyWeights.Queries;
using MediatR;

namespace GymTron.Api.Endpoints.BodyWeights;

public static class GetBodyWeightHistoryEndpoint
{
    public static void MapGetBodyWeightHistoryEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/bodyweights/history", async (ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            var history = await mediator.Send(new HistoryBodyWeightQuery(Guid.NewGuid(), user.GetUserId()), ct);
            return Results.Ok(history);
        })
        .WithTags("BodyWeights")
        .WithName("GetBodyWeightHistory")
        .WithSummary("Get body weight history");
    }
}

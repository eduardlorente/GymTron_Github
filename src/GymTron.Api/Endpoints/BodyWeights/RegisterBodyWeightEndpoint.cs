using System.Security.Claims;
using GymTron.Api.Infrastructure;
using GymTron.Application.BodyWeights.Commands;
using MediatR;

namespace GymTron.Api.Endpoints.BodyWeights;

public record RegisterBodyWeightRequest(decimal Weight, decimal BodyFatPercentage);

public static class RegisterBodyWeightEndpoint
{
    public static void MapRegisterBodyWeightEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/bodyweights", async (RegisterBodyWeightRequest request, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            var command = new RegisterBodyWeightCommand(Guid.NewGuid(), request.Weight, request.BodyFatPercentage, user.GetUserId());
            await mediator.Send(command, ct);
            return Results.Created();
        })
        .WithTags("BodyWeights")
        .WithName("RegisterBodyWeight")
        .WithSummary("Register a new body weight record");
    }
}

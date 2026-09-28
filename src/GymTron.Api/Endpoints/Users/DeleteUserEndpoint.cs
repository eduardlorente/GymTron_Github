using System.Security.Claims;
using GymTron.Api.Infrastructure;
using GymTron.Application.Users.Commands;
using MediatR;

namespace GymTron.Api.Endpoints.Users;

public static class DeleteUserEndpoint
{
    public static void MapDeleteUserEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/users/{id:int}", async (int id, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            var command = new DeleteUserCommand(Guid.NewGuid(), id, user.GetUserId());
            await mediator.Send(command, ct);
            return Results.NoContent();
        })
        .WithTags("Users")
        .WithName("DeleteUser")
        .WithSummary("Soft delete a user (Admin only)")
        .RequireAuthorization(policy => policy.RequireRole("Administrator"));
    }
}

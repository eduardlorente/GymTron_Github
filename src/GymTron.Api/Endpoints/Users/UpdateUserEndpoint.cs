using GymTron.Application.Users.Commands;
using GymTron.Domain.Enums;
using MediatR;

namespace GymTron.Api.Endpoints.Users;

public record UpdateUserRequest(string Username, string Email, UserTypes TypeId, bool IsActive, string? NewPassword = null);

public static class UpdateUserEndpoint
{
    public static void MapUpdateUserEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/users/{id:int}", async (int id, UpdateUserRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var command = new UpdateUserCommand(Guid.NewGuid(), id, request.Username, request.Email, request.TypeId, request.IsActive, request.NewPassword);
            await mediator.Send(command, ct);
            return Results.NoContent();
        })
        .WithTags("Users")
        .WithName("UpdateUser")
        .WithSummary("Update user details (Admin only)")
        .RequireAuthorization(policy => policy.RequireRole("Administrator"));
    }
}

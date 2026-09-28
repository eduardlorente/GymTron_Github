using GymTron.Application.Users.Commands;
using GymTron.Domain.Enums;
using MediatR;

namespace GymTron.Api.Endpoints.Users;

public record CreateUserRequest(string Username, string Email, string Password, UserTypes TypeId);

public static class CreateUserEndpoint
{
    public static void MapCreateUserEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/users", async (CreateUserRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var command = new CreateUserCommand(Guid.NewGuid(), request.Username, request.Email, request.Password, request.TypeId);
            var id = await mediator.Send(command, ct);
            return Results.Created($"/api/users/{id}", new { id });
        })
        .WithTags("Users")
        .WithName("CreateUser")
        .WithSummary("Create a new user (Admin only)")
        .RequireAuthorization(policy => policy.RequireRole("Administrator"));
    }
}

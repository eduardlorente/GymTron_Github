using System.Security.Claims;
using GymTron.Api.Infrastructure;
using GymTron.Application.Backup.Queries;
using MediatR;

namespace GymTron.Api.Endpoints.Backup;

public static class ExportBackupEndpoint
{
    public static void MapExportBackupEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/backup", async (ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var backup = await mediator.Send(new ExportUserDataQuery(Guid.NewGuid(), userId), ct);
            return Results.Ok(backup);
        })
        .WithTags("Backup")
        .WithName("ExportBackup")
        .WithSummary("Export all authenticated user data as a backup");
    }
}

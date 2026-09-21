using GymTron.Application.Base;
using GymTron.Application.Routines.Queries.DTO;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;

namespace GymTron.Application.Routines.Queries.Handlers;

internal class GetRoutineByIdQueryHandler(IRoutineRepository routineRepository, IExceptionLogger<GetRoutineByIdQuery> logger)
    : BaseQueryHandler<GetRoutineByIdQuery, RoutineDto?>(logger)
{
    private readonly IRoutineRepository _routineRepository = routineRepository;

    protected override async Task<RoutineDto?> HandleQuery(GetRoutineByIdQuery request, CancellationToken cancellationToken)
    {
        var projection = await _routineRepository.GetRoutineProjection(request.RoutineId, cancellationToken);
        
        if (projection == null)
            return null;

        if (request.UserId.HasValue && projection.UserId.HasValue && projection.UserId.Value != request.UserId.Value)
            return null;

        return new RoutineDto
        {
            Id = projection.Id,
            UserId = projection.UserId,
            Name = projection.Name,
            Items = projection.Items.Select(item => new RoutineItemDto
            {
                Id = item.Id,
                DayOfWeek = item.DayOfWeek,
                ExerciseParametersId = item.ExerciseParametersId,
                ExerciseName = item.ExerciseName,
                Series = item.Series,
                RepetitionsMin = item.RepetitionsMin,
                RepetitionsMax = item.RepetitionsMax,
                Duration = item.Duration,
                MinRestTimeInSeconds = item.MinRestTimeInSeconds,
                MaxRestTimeInSeconds = item.MaxRestTimeInSeconds,
                AlternatingSeries = item.AlternatingSeries,
                Position = item.Position,
                Type = item.Type
            }).ToList()
        };
    }
}

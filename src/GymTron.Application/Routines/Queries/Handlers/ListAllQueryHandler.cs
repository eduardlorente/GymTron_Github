using GymTron.Application.Base;
using GymTron.Application.Routines.Queries.DTO;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;

namespace GymTron.Application.Routines.Queries.Handlers;

internal class ListAllQueryHandler(IRoutineRepository routineRepository, IExceptionLogger<ListAllQuery> logger)
    : BaseQueryHandler<ListAllQuery, List<RoutineDto>>(logger)
{
    private readonly IRoutineRepository _routineRepository = routineRepository;

    protected override async Task<List<RoutineDto>> HandleQuery(ListAllQuery request, CancellationToken cancellationToken)
    {
        var projections = await _routineRepository.ListRoutineProjections(request.UserId, cancellationToken);

        if (projections == null || projections.Count == 0)
        {
            return [];
        }

        return projections.Select(routine => new RoutineDto
        {
            Id = routine.Id,
            UserId = routine.UserId,
            Name = routine.Name,
            Items = routine.Items.Select(item => new RoutineItemDto
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
        }).ToList();
    }
}

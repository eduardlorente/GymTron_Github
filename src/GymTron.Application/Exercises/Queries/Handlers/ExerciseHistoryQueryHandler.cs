using GymTron.Application.Base;
using GymTron.Domain.Entities;
using GymTron.Domain.Projections;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;

namespace GymTron.Application.Exercises.Queries.Handlers;

internal class ExerciseHistoryQueryHandler(IExerciseRepository exerciseRepository, IExceptionLogger<ExerciseHistoryQuery> logger)
    : BaseQueryHandler<ExerciseHistoryQuery, List<ExerciseHistoryProjection>>(logger)
{


    private readonly IExerciseRepository _exerciseRepository = exerciseRepository;


    protected override async Task<List<ExerciseHistoryProjection>> HandleQuery(ExerciseHistoryQuery request, CancellationToken cancellationToken)
    {
        return await _exerciseRepository.ListHistory(request.UserId, cancellationToken);
    }
}

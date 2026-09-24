using GymTron.Application.Backup.DTOs;
using GymTron.Application.Base;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;

namespace GymTron.Application.Backup.Queries.Handlers;

internal class ExportUserDataQueryHandler(
    IRoutineRepository routineRepository,
    ITrainingRepository trainingRepository,
    IExerciseRepository exerciseRepository,
    IBodyWeightRepository bodyWeightRepository,
    IClock clock,
    IExceptionLogger<ExportUserDataQuery> logger)
    : BaseQueryHandler<ExportUserDataQuery, UserDataBackupDto>(logger)
{
    private readonly IRoutineRepository _routineRepository = routineRepository;
    private readonly ITrainingRepository _trainingRepository = trainingRepository;
    private readonly IExerciseRepository _exerciseRepository = exerciseRepository;
    private readonly IBodyWeightRepository _bodyWeightRepository = bodyWeightRepository;
    private readonly IClock _clock = clock;

    protected override async Task<UserDataBackupDto> HandleQuery(ExportUserDataQuery request, CancellationToken cancellationToken)
    {
        var routinesTask = _routineRepository.ListRoutineProjections(request.UserId, cancellationToken);
        var trainingsTask = _trainingRepository.ListCompletedHistory(request.UserId, cancellationToken);
        var exercisesTask = _exerciseRepository.ListHistory(request.UserId, cancellationToken);
        var bodyWeightsTask = _bodyWeightRepository.ListHistory(request.UserId, cancellationToken);

        await Task.WhenAll(routinesTask, trainingsTask, exercisesTask, bodyWeightsTask);

        return new UserDataBackupDto
        {
            Version = 1,
            ExportedAtUtc = _clock.UtcNow,
            UserId = request.UserId,
            Routines = await routinesTask,
            CompletedTrainings = await trainingsTask,
            ExerciseHistory = await exercisesTask,
            BodyWeights = await bodyWeightsTask
        };
    }
}

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
        var routines = await _routineRepository.ListRoutineProjections(request.UserId, cancellationToken);
        var trainings = await _trainingRepository.ListCompletedHistory(request.UserId, cancellationToken);
        var exercises = await _exerciseRepository.ListHistory(request.UserId, cancellationToken);
        var bodyWeights = await _bodyWeightRepository.ListHistory(request.UserId, cancellationToken);

        return new UserDataBackupDto
        {
            Version = 1,
            ExportedAtUtc = _clock.UtcNow,
            UserId = request.UserId,
            Routines = routines,
            CompletedTrainings = trainings,
            ExerciseHistory = exercises,
            BodyWeights = bodyWeights
        };
    }
}

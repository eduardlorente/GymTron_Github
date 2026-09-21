using GymTron.Application.Base;
using GymTron.Application.Trainings.Queries.DTO;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;

namespace GymTron.Application.Trainings.Queries.Handlers;

internal class CurrentTrainingQueryHandler(ITrainingRepository trainingRepository, IExceptionLogger<CurrentTrainingQuery> logger)
    : BaseQueryHandler<CurrentTrainingQuery, TrainingDto?>(logger)
{
    private readonly ITrainingRepository _trainingRepository = trainingRepository;

    protected override async Task<TrainingDto?> HandleQuery(CurrentTrainingQuery request, CancellationToken cancellationToken)
    {
        var training = await _trainingRepository.GetCurrent(request.UserId, cancellationToken);
        if (training == null)
        {
            return null;
        }

        return new TrainingDto
        {
            Id = training.Id,
            UserId = training.UserId,
            RoutineId = training.RoutineId,
            DayOfTheWeek = training.DayOfTheWeek,
            StartedOn = training.StartedOn.FullDate,
            CompletedOn = training.CompletedOn?.FullDate,
            StatusId = (int)training.Status.Status,
            PendingWorkout = training.PendingWorkout.Select(item => new TrainingRoutineItemDto
            {
                Id = item.Id,
                DayOfWeek = item.DayOfWeek,
                ExerciseParametersId = item.ExerciseParameters.Id,
                ExerciseName = item.ExerciseParameters.Name,
                Series = item.ExerciseParameters.Series,
                RepetitionsMin = item.ExerciseParameters.Repetitions.Min,
                RepetitionsMax = item.ExerciseParameters.Repetitions.Max,
                Duration = item.ExerciseParameters.DurationInSeconds,
                MinRestTimeInSeconds = item.ExerciseParameters.RestTimeInSeconds.Min,
                MaxRestTimeInSeconds = item.ExerciseParameters.RestTimeInSeconds.Max,
                AlternatingSeries = item.AlternatingSeries,
                Position = item.Position,
                Type = (int)item.ExerciseParameters.Type
            }).ToList(),
            CompletedWorkout = training.CompletedWorkout.Select(exercise => new TrainingExerciseDto
            {
                Id = exercise.Id,
                TrainingId = exercise.TrainingId,
                ExerciseParametersId = exercise.ExerciseParametersId,
                Name = exercise.Name,
                Weight = exercise.Weight,
                DurationInSeconds = exercise.DurationInSeconds,
                Repetitions = exercise.CurrentRepetitions,
                Observations = exercise.Observations.Select(o => o.Comment).ToList(),
                CreatedOn = exercise.Status.CreatedOn
            }).ToList()
        };
    }
}

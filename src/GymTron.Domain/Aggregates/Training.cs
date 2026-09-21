using GymTron.Domain.Entities;
using GymTron.Domain.Enums;
using GymTron.Domain.Exceptions;
using GymTron.Domain.Services;
using GymTron.Domain.ValueObjects;

namespace GymTron.Domain.Aggregates;

public class Training : AggregateRoot<int>
{
    public int? UserId { get; private set; }
    public int RoutineId { get; private set; }
    public int DayOfTheWeek { get; private set; }
    public TrainingDate StartedOn { get; private set; } = default!;
    public TrainingDate? CompletedOn { get; private set; } = null;
    private readonly List<RoutineItem> _pendingWorkout = [];
    private readonly List<Exercise> _completedWorkout = [];
    public IReadOnlyCollection<RoutineItem> PendingWorkout => _pendingWorkout;
    public IReadOnlyCollection<Exercise> CompletedWorkout => _completedWorkout;

    private Training(int? userId,
                     int routineId,
                     int dayOfTheWeek,
                     List<RoutineItem> pendingWorkout,
                     IClock clock)
    {
        Id = 0;
        UserId = userId;
        Status.Create(clock);

        RoutineId = routineId;
        DayOfTheWeek = dayOfTheWeek;
        _pendingWorkout = pendingWorkout;
        StartedOn = new TrainingDate(clock.UtcNow);
    }

    private Training(int trainingId,
                     int? userId,
                     int routineId,
                     int dayOfTheWeek,
                     DateTime startedOn,
                     DateTime? completedOn,
                     EntityStatusTypes statusType,
                     List<RoutineItem> pendingWorkout,
                     List<Exercise> completedWorkout)
    {
        Id = trainingId;
        UserId = userId;
        RoutineId = routineId;
        DayOfTheWeek = dayOfTheWeek;
        _pendingWorkout = pendingWorkout;
        _completedWorkout = completedWorkout;
        Status = EntityStatus.FromDatabase(statusType, startedOn);
        StartedOn = new(startedOn);

        if (completedOn.HasValue)
        {
            CompletedOn = new(completedOn.Value);
        }
    }

    public static Training CreateAnStartedTraining(int routineId,
                                                   int dayOfTheWeek,
                                                   List<RoutineItem> pendingWorkout,
                                                   IClock clock,
                                                   int? userId = null)
    {
        return new(userId,
                   routineId,
                   dayOfTheWeek,
                   pendingWorkout,
                   clock);
    }

    public static Training FromDatabase(int trainingId,
                                        int routineId,
                                        int dayOfWeek,
                                        DateTime startedOn,
                                        DateTime? completedOn,
                                        EntityStatusTypes statusType,
                                        List<RoutineItem> pendingWorkout,
                                        List<Exercise> completedWorkout,
                                        int? userId = null)
    {
        return new(trainingId,
                   userId,
                   routineId,
                   dayOfWeek,
                   startedOn,
                   completedOn,
                   statusType,
                   pendingWorkout,
                   completedWorkout);
    }

    public void CompleteExercise(Exercise exercise, IClock clock)
    {
        bool notExistsExerciseInTraining = !_completedWorkout.Any(x => x.ExerciseParametersId == exercise.ExerciseParametersId);

        if (notExistsExerciseInTraining)
        {
            _completedWorkout.Add(exercise);
            _pendingWorkout.RemoveAll(x => x.ExerciseParameters.Id == exercise.ExerciseParametersId);
            Status.Update(clock);
        }
    }

    public void Complete(IClock clock)
    {
        if (!Status.IsActive)
        {
            throw new InvalidDomainOperationException("Training already completed.");
        }

        Status.Update(EntityStatusTypes.COMPLETED, clock);
        CompletedOn = new TrainingDate(clock.UtcNow);
    }

    public void Cancel(IClock clock)
    {
        if (!Status.IsActive)
        {
            throw new InvalidDomainOperationException("Training already completed.");
        }

        Status.Update(EntityStatusTypes.CANCELLED, clock);
    }
}

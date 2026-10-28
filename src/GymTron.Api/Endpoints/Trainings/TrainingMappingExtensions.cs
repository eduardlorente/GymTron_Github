using GymTron.Application.Trainings.Queries.DTO;
using GymTron.Domain.Aggregates;
using GymTron.Domain.Entities;
using GymTron.Domain.Enums;
using GymTron.Domain.ValueObjects;

namespace GymTron.Api.Endpoints.Trainings;

public static class TrainingMappingExtensions
{
    public static Training ToDomain(this TrainingDto dto, int? userId = null)
    {
        return Training.FromDatabase(
            dto.Id,
            dto.RoutineId,
            dto.DayOfTheWeek,
            dto.StartedOn,
            dto.CompletedOn,
            (EntityStatusTypes)dto.StatusId,
            dto.PendingWorkout.Select(ri => RoutineItem.FromDatabase(
                ri.Id,
                ri.DayOfWeek,
                GymTron.Domain.Entities.ExerciseParameters.FromDatabase(
                    ri.ExerciseParametersId,
                    ri.ExerciseName,
                    ri.Description ?? string.Empty,
                    ri.Pattern ?? string.Empty,
                    ri.Series,
                    (ri.RepetitionsMin, ri.RepetitionsMax),
                    ri.Duration ?? 0,
                    ri.ReplaysInReserve,
                    (ri.MinRestTimeInSeconds, ri.MaxRestTimeInSeconds ?? 0),
                    ri.LastWeight,
                    ri.LastDuration,
                    ri.LastRepetitions,
                    (ExerciseTypes)ri.Type,
                    (ri.LastObservations ?? []).Select(o => new Observation(o)).ToList()),
                ri.AlternatingSeries,
                ri.Position)).ToList(),
            dto.CompletedWorkout.Select(e => Exercise.FromDatabase(
                e.Id,
                e.TrainingId > 0 ? e.TrainingId : dto.Id,
                e.ExerciseParametersId,
                e.Name,
                e.Weight,
                e.DurationInSeconds,
                e.Repetitions,
                e.CreatedOn != default ? e.CreatedOn : (dto.CompletedOn ?? dto.StartedOn),
                e.Observations)).ToList(),
            userId ?? dto.UserId
        );
    }

    public static TrainingDto ToDto(this Training training)
    {
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
                Description = item.ExerciseParameters.Description,
                Pattern = item.ExerciseParameters.Pattern,
                Series = item.ExerciseParameters.Series,
                RepetitionsMin = item.ExerciseParameters.Repetitions.Min,
                RepetitionsMax = item.ExerciseParameters.Repetitions.Max,
                Duration = item.ExerciseParameters.DurationInSeconds,
                ReplaysInReserve = item.ExerciseParameters.ReplaysInReserve,
                MinRestTimeInSeconds = item.ExerciseParameters.RestTimeInSeconds.Min,
                MaxRestTimeInSeconds = item.ExerciseParameters.RestTimeInSeconds.Max,
                AlternatingSeries = item.AlternatingSeries,
                Position = item.Position,
                Type = (int)item.ExerciseParameters.Type,
                LastWeight = item.ExerciseParameters.LastWeight,
                LastRepetitions = item.ExerciseParameters.LastRepetitions,
                LastDuration = item.ExerciseParameters.LastDurationInSeconds > 0 ? item.ExerciseParameters.LastDurationInSeconds : null,
                LastObservations = item.ExerciseParameters.Observations.Select(o => o.Comment).ToList()
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

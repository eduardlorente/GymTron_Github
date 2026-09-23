using GymTron.Domain.Projections;

namespace GymTron.Application.Backup.DTOs;

public sealed record UserDataBackupDto
{
    public int Version { get; init; } = 1;
    public DateTime ExportedAtUtc { get; init; } = DateTime.UtcNow;
    public int? UserId { get; init; }
    public List<RoutineProjection> Routines { get; init; } = [];
    public List<TrainingHistoryProjection> CompletedTrainings { get; init; } = [];
    public List<ExerciseHistoryProjection> ExerciseHistory { get; init; } = [];
    public List<BodyWeightHistoryProjection> BodyWeights { get; init; } = [];
}

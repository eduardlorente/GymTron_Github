using GymTron.App.Services.Api.Models;

namespace GymTron.App.Services.Api;

public interface IGymTronApiClient
{
    Task<List<RoutineDto>> GetRoutinesAsync(CancellationToken ct = default);
    Task<List<ExerciseHistoryItemDto>> GetExerciseHistoryAsync(CancellationToken ct = default);
    Task<List<BodyWeightHistoryDto>> GetBodyWeightHistoryAsync(CancellationToken ct = default);
    Task RegisterBodyWeightAsync(decimal weight, decimal bodyFatPercentage, CancellationToken ct = default);
    Task<TrainingDto?> GetCurrentTrainingAsync(CancellationToken ct = default);
    Task<List<TrainingHistoryDto>> GetTrainingHistoryAsync(CancellationToken ct = default);
    Task StartTrainingAsync(int routineId, int dayOfWeek, CancellationToken ct = default);
    Task FinishTrainingAsync(TrainingDto training, CancellationToken ct = default);
    Task CancelTrainingAsync(TrainingDto training, CancellationToken ct = default);
    Task<TrainingDto?> AddExerciseToTrainingAsync(AddExerciseToTrainingRequest request, CancellationToken ct = default);
}

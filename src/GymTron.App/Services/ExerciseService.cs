using GymTron.App.Services.Api;
using GymTron.App.ViewModels.Entities;

namespace GymTron.App.Services;

internal class ExerciseService(IGymTronApiClient apiClient) : IExerciseService
{
    private readonly IGymTronApiClient _apiClient = apiClient;

    public async Task<List<ExerciseViewModel>> ListAll()
    {
        var exercises = await _apiClient.GetExerciseHistoryAsync();

        return [.. exercises.Select(x => new ExerciseViewModel(x))];
    }
}

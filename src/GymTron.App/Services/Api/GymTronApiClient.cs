using System.Net.Http.Json;
using GymTron.App.Services.Api.Models;

namespace GymTron.App.Services.Api;

public class GymTronApiClient(HttpClient httpClient) : IGymTronApiClient
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<List<RoutineDto>> GetRoutinesAsync(CancellationToken ct = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<RoutineDto>>("api/routines", ct);
        return result ?? [];
    }

    public async Task<List<ExerciseHistoryItemDto>> GetExerciseHistoryAsync(CancellationToken ct = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<ExerciseHistoryItemDto>>("api/exercises/history", ct);
        return result ?? [];
    }

    public async Task<List<BodyWeightHistoryDto>> GetBodyWeightHistoryAsync(CancellationToken ct = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<BodyWeightHistoryDto>>("api/bodyweights/history", ct);
        return result ?? [];
    }

    public async Task RegisterBodyWeightAsync(decimal weight, decimal bodyFatPercentage, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/bodyweights", new RegisterBodyWeightRequest(weight, bodyFatPercentage), ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<TrainingDto?> GetCurrentTrainingAsync(CancellationToken ct = default)
    {
        return await _httpClient.GetFromJsonAsync<TrainingDto?>("api/trainings/current", ct);
    }

    public async Task<List<TrainingHistoryDto>> GetTrainingHistoryAsync(CancellationToken ct = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<TrainingHistoryDto>>("api/trainings/history", ct);
        return result ?? [];
    }

    public async Task StartTrainingAsync(int routineId, int dayOfWeek, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/trainings/start", new StartTrainingRequest(routineId, dayOfWeek), ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task FinishTrainingAsync(TrainingDto training, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/trainings/finish", new FinishTrainingRequest(training), ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task CancelTrainingAsync(TrainingDto training, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/trainings/cancel", new CancelTrainingRequest(training), ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<TrainingDto?> AddExerciseToTrainingAsync(AddExerciseToTrainingRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/trainings/exercises", request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TrainingDto>(cancellationToken: ct);
    }
}

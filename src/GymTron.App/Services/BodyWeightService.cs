using GymTron.App.Services.Api;
using GymTron.App.ViewModels.Entities;

namespace GymTron.App.Services;

internal class BodyWeightService(IGymTronApiClient apiClient) : IBodyWeightService
{
    private readonly IGymTronApiClient _apiClient = apiClient;

    public async Task RegisterBodyWeight(decimal weight, decimal bodyFatPercentage)
    {
        await _apiClient.RegisterBodyWeightAsync(weight, bodyFatPercentage);
    }

    public async Task<List<BodyWeightHistoryItemViewModel>> ListHistory()
    {
        var bodyWeights = await _apiClient.GetBodyWeightHistoryAsync();

        return [.. bodyWeights.Select(bw => new BodyWeightHistoryItemViewModel(bw.Weight, bw.BodyFatPercentage, bw.CreatedOn))];
    }
}

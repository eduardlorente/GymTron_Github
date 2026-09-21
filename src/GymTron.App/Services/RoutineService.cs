using GymTron.App.Services.Api;
using GymTron.App.ViewModels.Entities;

namespace GymTron.App.Services;

internal class RoutineService(IGymTronApiClient apiClient) : IRoutineService
{
    private readonly IGymTronApiClient _apiClient = apiClient;

    public async Task<List<RoutineViewModel>> ListAllRoutines()
    {
        var routines = await _apiClient.GetRoutinesAsync();

        return [.. routines.OrderByDescending(r => r.Id).Take(3).Select(r => new RoutineViewModel(r))];
    }
}

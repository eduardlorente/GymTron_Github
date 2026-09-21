using GymTron.App.ViewModels.Entities;

namespace GymTron.App.Services;

public interface IBodyWeightService
{
    Task RegisterBodyWeight(decimal weight, decimal bodyFatPercentage);
    Task<List<BodyWeightHistoryItemViewModel>> ListHistory();
}
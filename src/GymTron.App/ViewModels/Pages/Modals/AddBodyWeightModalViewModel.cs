using GymTron.App.Services;
using System.Windows.Input;

namespace GymTron.App.ViewModels.Pages.Modals;

public class AddBodyWeightModalViewModel : PageBaseViewModel
{


    private readonly IBodyWeightService _bodyWeightService;
    private readonly Func<Task> _onAddBodyWeight;

    public string Weight { get; set; } = string.Empty;
    public string BodyFatPercentage { get; set; } = string.Empty;

    public ICommand AddBodyWeightCommand { get; }

    public AddBodyWeightModalViewModel(IBodyWeightService bodyWeightService, Func<Task> onAddBodyWeight)
    {
        _bodyWeightService = bodyWeightService;
        _onAddBodyWeight = onAddBodyWeight;

        AddBodyWeightCommand = new Command(async () => await OnAddBodyWeight());
    }

    private async Task OnAddBodyWeight()
    {
        if (string.IsNullOrWhiteSpace(Weight) || !decimal.TryParse(Weight, out decimal weight))
            return;

        if (string.IsNullOrWhiteSpace(BodyFatPercentage) || !decimal.TryParse(BodyFatPercentage, out decimal bodyFat))
        {
            bodyFat = 0;
        }

        await _bodyWeightService.RegisterBodyWeight(weight, bodyFat);
        await _onAddBodyWeight();
    }
}


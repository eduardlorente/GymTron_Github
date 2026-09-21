using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using GymTron.App.Pages.Modals;
using GymTron.App.Services;
using GymTron.App.ViewModels.Entities;
using GymTron.App.ViewModels.Pages.Modals;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace GymTron.App.ViewModels.Pages;

public partial class BodyWeightsHistoryPageViewModel : PageBaseViewModel
{
    private readonly IBodyWeightService _bodyWeightService;

    public string Title => LocalizationService.GetString("BodyWeights_Title");

    public ObservableCollection<BodyWeightHistoryItemViewModel> BodyWeightsHistoryItems { get; set; } = [];

    public ICommand AddBodyWeightCommand { get; }


    public BodyWeightsHistoryPageViewModel(IBodyWeightService bodyWeightService)
    {
        _bodyWeightService = bodyWeightService;

        _ = LoadTrainingHistoryAsync();

        AddBodyWeightCommand = new Command(async () => await OnAddBodyWeight());
    }


    private async Task LoadTrainingHistoryAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            List<BodyWeightHistoryItemViewModel> historyItems = await _bodyWeightService.ListHistory();

            BodyWeightsHistoryItems.Clear();

            for (int i = 0; i < historyItems.Count; i++)
            {
                BodyWeightHistoryItemViewModel item = historyItems[i];
                BodyWeightHistoryItemViewModel? nextItem = i < historyItems.Count - 1 ? historyItems[i + 1] : null;

                item.SetColor(nextItem!);
                BodyWeightsHistoryItems.Add(item);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load body weight history: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }


    private async Task OnAddBodyWeight()
    {
        static async Task onAddBodyWeight()
        {
            await Shell.Current.Navigation.PopModalAsync();
            await Shell.Current.Navigation.PopAsync();

            IToast toast = Toast.Make("Enhorabona, nova mesura registrada correctament!");
            await toast.Show();
        }

        var model = new AddBodyWeightModalViewModel(_bodyWeightService, onAddBodyWeight);

        await Shell.Current.Navigation.PushModalAsync(new AddBodyWeightModal(model));
    }
}

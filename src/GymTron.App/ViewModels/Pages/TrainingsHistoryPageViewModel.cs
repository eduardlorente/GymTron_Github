using GymTron.App.Services;
using GymTron.App.ViewModels.Entities;
using System.Collections.ObjectModel;

namespace GymTron.App.ViewModels.Pages;

public partial class TrainingsHistoryPageViewModel : PageBaseViewModel
{
    private readonly ITrainingService _trainingService;

    public string Title => LocalizationService.GetString("TrainingsHistory_Title");
    public string TotalFormat => LocalizationService.GetString("TrainingsHistory_TotalFormat");
    public string DayFormat => LocalizationService.GetString("TrainingsHistory_DayFormat");
    public string CountFormat => LocalizationService.GetString("TrainingsHistory_CountFormat");

    public ObservableCollection<TrainingHistoryItemViewModel> TrainingHistoryItems { get; set; } = [];


    public TrainingsHistoryPageViewModel(ITrainingService trainingService)
    {
        _trainingService = trainingService;

        _ = LoadTrainingHistoryAsync();
    }


    private async Task LoadTrainingHistoryAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            List<TrainingHistoryItemViewModel> historyItems = await _trainingService.ListHistory();

            TrainingHistoryItems.Clear();
            foreach (TrainingHistoryItemViewModel item in historyItems)
            {
                TrainingHistoryItems.Add(item);
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            // Session expired; AuthHttpMessageHandler navigates to Login, suppress alert
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load training history: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}

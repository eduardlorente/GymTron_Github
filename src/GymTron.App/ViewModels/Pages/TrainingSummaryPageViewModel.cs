using GymTron.App.Services;
using GymTron.App.ViewModels.Entities;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace GymTron.App.ViewModels.Pages;

public class TrainingSummaryPageViewModel : PageBaseViewModel, IQueryAttributable
{
    public string Title => LocalizationService.GetString("TrainingSummary_Title");
    public string CongratulationsText => LocalizationService.GetString("TrainingSummary_Congratulations");
    public string DurationLabel => LocalizationService.GetString("TrainingSummary_Duration");
    public string ExercisesLabel => LocalizationService.GetString("TrainingSummary_Exercises");
    public string TotalVolumeLabel => LocalizationService.GetString("TrainingSummary_TotalVolume");
    public string TotalRepsLabel => LocalizationService.GetString("TrainingSummary_TotalReps");
    public string CompletedExercisesLabel => LocalizationService.GetString("TrainingSummary_CompletedExercises");
    public string PendingExercisesLabel => LocalizationService.GetString("TrainingSummary_PendingExercises");
    public string DoneButtonText => LocalizationService.GetString("TrainingSummary_Done");

    private TrainingSummaryModel? _summary;
    public TrainingSummaryModel? Summary
    {
        get => _summary;
        private set
        {
            if (SetProperty(ref _summary, value))
            {
                OnPropertyChanged(nameof(DurationDisplay));
                OnPropertyChanged(nameof(ExercisesDisplay));
                OnPropertyChanged(nameof(VolumeDisplay));
                OnPropertyChanged(nameof(RepsDisplay));
                OnPropertyChanged(nameof(HasPendingExercises));
                UpdateCollections();
            }
        }
    }

    public string DurationDisplay => Summary?.FormattedDuration ?? "00:00:00";
    public string ExercisesDisplay => Summary != null ? $"{Summary.CompletedExercisesCount} / {Summary.TotalPlannedExercises}" : "0 / 0";
    public string VolumeDisplay => Summary != null ? $"{Summary.TotalVolumeKg:G29} kg" : "0 kg";
    public string RepsDisplay => Summary?.TotalRepetitions.ToString() ?? "0";
    public bool HasPendingExercises => Summary?.HasPendingExercises ?? false;

    public ObservableCollection<CompletedExerciseSummaryItem> CompletedExercises { get; } = [];
    public ObservableCollection<string> PendingExercises { get; } = [];

    public ICommand DoneCommand { get; }

    public TrainingSummaryPageViewModel()
    {
        DoneCommand = new Command(async () => await OnDone());
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Summary", out object? summaryObj) && summaryObj is TrainingSummaryModel summary)
        {
            Summary = summary;
        }
    }

    private void UpdateCollections()
    {
        CompletedExercises.Clear();
        PendingExercises.Clear();

        if (Summary != null)
        {
            foreach (var item in Summary.CompletedExercises)
            {
                CompletedExercises.Add(item);
            }

            foreach (var item in Summary.PendingExercises)
            {
                PendingExercises.Add(item);
            }
        }
    }

    private async Task OnDone()
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
}

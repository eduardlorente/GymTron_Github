using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using GymTron.App.Extensions;
using GymTron.App.Pages;
using GymTron.App.Services;
using GymTron.App.ViewModels.Entities;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace GymTron.App.ViewModels.Pages;

public partial class CurrentTrainingPageViewModel : PageBaseViewModel
{
    private readonly ITrainingService _trainingService;
    private TrainingViewModel? _currentTraining;
    private CancellationTokenSource? _timerCancellationTokenSource;

    public string Title => LocalizationService.GetString("CurrentTraining_Title");

    public DateTime StartTime => _currentTraining?.StartedOn.FullDate ?? DateTime.MinValue;

    public string ElapsedTime
    {
        get
        {
            TimeSpan elapsed = DateTime.Now - StartTime;
            return $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
        }
    }

    public ObservableCollection<ExerciseItemViewModel> TrainingExercises { get; set; }
    public ICommand FinishTrainingCommand { get; }
    public ICommand SelectExerciseCommand { get; }
    public ICommand CancelTrainingCommand { get; }


    public CurrentTrainingPageViewModel(ITrainingService trainingService)
    {
        _trainingService = trainingService;

        TrainingExercises = [];

        LoadCurrentTrainingAsync().SafeFireAndForget();
        StartTimer();

        FinishTrainingCommand = new Command(async () => await FinishTraining());
        SelectExerciseCommand = new Command<ExerciseItemViewModel>(exercise => OnExerciseSelectedAsync(exercise).SafeFireAndForget());
        CancelTrainingCommand = new Command(async () => await CancelTraining());
    }


    private async Task LoadCurrentTrainingAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            _currentTraining = await _trainingService.GetCurrentTraining();
            if (_currentTraining != null)
            {
                List<ExerciseItemViewModel> exercises = [];

                foreach (RoutineItemViewModel item in _currentTraining.PendingWorkout)
                {
                    exercises.Add(new ExerciseItemViewModel(item.ExerciseParameters.Id,
                                                            item.ExerciseParameters.Name,
                                                            item.ExerciseParameters.Description,
                                                            false,
                                                            item.AlternatingSeries));
                }

                foreach (ExerciseViewModel exercise in _currentTraining.CompletedWorkout)
                {
                    exercises.Add(new ExerciseItemViewModel(exercise.ExerciseParametersId, exercise.Name, true, false));
                }

                exercises = [.. exercises.OrderByDescending(e => e.IsCompleted)];

                TrainingExercises.Clear();
                foreach (ExerciseItemViewModel exercise in exercises)
                {
                    TrainingExercises.Add(exercise);
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load current training: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }


    private void StartTimer()
    {
        _timerCancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = _timerCancellationTokenSource.Token;

        Task.Run(async () =>
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    MainThread.BeginInvokeOnMainThread(() => OnPropertyChanged(nameof(ElapsedTime)));
                    await Task.Delay(1000, token);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when timer is cancelled
            }
        }, token).SafeFireAndForget();
    }


    private async Task FinishTraining()
    {
        bool isConfirmed = await Shell.Current.DisplayAlert(
            LocalizationService.GetString("Dialog_Confirmation"),
            LocalizationService.GetString("CurrentTraining_ConfirmFinish"),
            LocalizationService.GetString("Dialog_Yes"),
            LocalizationService.GetString("Dialog_No")
        );

        if (isConfirmed)
        {
            if (_currentTraining == null)
                return;

            TrainingSummaryModel summary = TrainingSummaryModel.FromTraining(_currentTraining, DateTime.Now);

            await _trainingService.FinalizeTraining();
            _timerCancellationTokenSource?.Cancel();

            await Shell.Current.GoToAsync(
                nameof(TrainingSummaryPage),
                new Dictionary<string, object>
                {
                    { "Summary", summary }
                });
        }
    }


    private async Task OnExerciseSelectedAsync(ExerciseItemViewModel selectedExercise)
    {
        if (selectedExercise == null || _currentTraining == null)
            return;

        if (_currentTraining.CompletedWorkout.Any(e => e.Name == selectedExercise.Name))
            return;

        try
        {
            await Shell.Current.GoToAsync(
                nameof(ExerciseDetailPage),
                new Dictionary<string, object>
                {
                { "ExerciseParametersId", selectedExercise.ExerciseParametersId }
                });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to navigate to exercise detail: {ex.Message}", "OK");
        }
    }


    private async Task CancelTraining()
    {
        bool isConfirmed = await Shell.Current.DisplayAlert(
            LocalizationService.GetString("Dialog_Confirmation"),
            LocalizationService.GetString("CurrentTraining_ConfirmCancel"),
            LocalizationService.GetString("Dialog_Yes"),
            LocalizationService.GetString("Dialog_No")
        );

        if (isConfirmed)
        {
            if (_currentTraining == null)
                return;

            await _trainingService.CancelTraining();
            _timerCancellationTokenSource?.Cancel();

            IToast toast = Toast.Make(LocalizationService.GetString("CurrentTraining_CancelledToast"));
            await toast.Show();

            await Shell.Current.GoToAsync("//MainPage");
        }
    }


    internal void ReloadData()
    {
        LoadCurrentTrainingAsync().SafeFireAndForget();
        StartTimer();
    }
}


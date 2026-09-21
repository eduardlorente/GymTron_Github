using GymTron.App.Pages;
using GymTron.App.Pages.Modals;
using GymTron.App.Services;
using GymTron.App.Services.Api.Models;
using GymTron.App.ViewModels.Entities;
using GymTron.App.ViewModels.Pages.Modals;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace GymTron.App.ViewModels.Pages;

public class ExerciseDetailPageViewModel : PageBaseViewModel, IQueryAttributable
{
    private readonly ITrainingService _trainingService;

    public string Title => LocalizationService.GetString("ExerciseDetail_Exercise");

    private int _exerciseParametersId;

    private string exerciseName = string.Empty;
    public string ExerciseName
    {
        get => exerciseName;
        set => SetProperty(ref exerciseName, value);
    }

    private string description = string.Empty;
    public string Description
    {
        get => description;
        set => SetProperty(ref description, value);
    }

    private string repetitionsTitle = string.Empty;
    public string RepetitionsTitle
    {
        get => repetitionsTitle;
        set => SetProperty(ref repetitionsTitle, value);
    }

    private string repetitions = string.Empty;
    public string Repetitions
    {
        get => repetitions;
        set => SetProperty(ref repetitions, value);
    }

    private string series = string.Empty;
    public string Series
    {
        get => series;
        set => SetProperty(ref series, value);
    }

    private bool alternatingSeries = false;
    public bool AlternatingSeries
    {
        get => alternatingSeries;
        set => SetProperty(ref alternatingSeries, value);
    }

    private string pattern = string.Empty;
    public string Pattern
    {
        get => pattern;
        set => SetProperty(ref pattern, value);
    }

    private string replaysInReserve = string.Empty;
    public string ReplaysInReserve
    {
        get => replaysInReserve;
        set => SetProperty(ref replaysInReserve, value);
    }

    private string _restTime = string.Empty;
    public string RestTime
    {
        get => _restTime;
        set => SetProperty(ref _restTime, value);
    }

    private decimal _lastWeight;
    private string lastWeightWithSufix = string.Empty;
    public string? LastWeightWithSuffix
    {
        get => lastWeightWithSufix;
        set => SetProperty(ref lastWeightWithSufix, value);
    }

    private string lastRepetitions = string.Empty;
    public string? LastRepetitions
    {
        get => lastRepetitions;
        set => SetProperty(ref lastRepetitions, value);
    }

    private int _lastDurationInSeconds;
    private string lastDurationWithSufix = string.Empty;
    public string? LastDurationWithSufix
    {
        get => lastDurationWithSufix;
        set => SetProperty(ref lastDurationWithSufix, value);
    }

    private bool isDurationExercise;
    public bool IsDurationExercise
    {
        get => isDurationExercise;
        set => SetProperty(ref isDurationExercise, value);
    }

    private ObservableCollection<string> _observations;
    public ObservableCollection<string> Observations
    {
        get => _observations;
        set => SetProperty(ref _observations, value);
    }

    public ICommand CompleteExerciseCommand { get; }


    public ExerciseDetailPageViewModel(ITrainingService trainingService)
    {
        _trainingService = trainingService;
        CompleteExerciseCommand = new Command(async () => await OnCompleteExercise());
    }


    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("ExerciseParametersId", out object? id) && id is int exerciseParamtersId)
        {
            _exerciseParametersId = exerciseParamtersId;
            _ = LoadExerciseDetailsAsync();
        }
    }


    private async Task LoadExerciseDetailsAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            TrainingViewModel? currentTraining = await _trainingService.GetCurrentTraining();

            if (currentTraining == null)
            {
                await Shell.Current.DisplayAlert(
                    "Error",
                    LocalizationService.GetString("ExerciseDetail_NoTrainingInProgress"),
                    LocalizationService.GetString("Dialog_Ok")
                );
                return;
            }

            RoutineItemViewModel? routineItem = currentTraining
                .PendingWorkout
                .FirstOrDefault(item => item.ExerciseParameters.Id == _exerciseParametersId);

            if (routineItem != null)
            {
                ExerciseParametersViewModel? exercise = routineItem?.ExerciseParameters;

                if (exercise != null)
                {
                    ExerciseName = exercise.Name;
                    Description = exercise.Description;
                    IsDurationExercise = exercise.TypeId == (int)ExerciseTypes.DURATION;
                    Series = exercise.Series.ToString();
                    Pattern = exercise.Pattern;
                    AlternatingSeries = routineItem.AlternatingSeries;
                    Observations = new ObservableCollection<string>(exercise.Observations.Select(o => o.Comment));
                    RestTime = BuildRestTimeLiteral(routineItem);

                    if (IsDurationExercise)
                    {
                        _lastDurationInSeconds = exercise.LastDurationInSeconds ?? 0;
                        LastDurationWithSufix = $"{_lastDurationInSeconds} segons";
                        RepetitionsTitle = "Duració:";
                        Repetitions = $"{exercise.DurationInSeconds} segons";
                    }
                    else
                    {
                        _lastWeight = exercise.LastWeight ?? 0;
                        LastWeightWithSuffix = $"{_lastWeight} kg";
                        LastRepetitions = exercise.LastRepetitions.HasValue ? exercise.LastRepetitions.Value.ToString() : "0";
                        RepetitionsTitle = "Repeticions:";
                        Repetitions = $"{exercise.Repetitions.Min} - {exercise.Repetitions.Max}";
                        ReplaysInReserve = exercise.ReplaysInReserve.HasValue ? exercise.ReplaysInReserve.Value.ToString() : "0";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load exercise details: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }


    private async Task OnCompleteExercise()
    {
        static async Task onCompleted()
        {
            _ = await Shell.Current.Navigation.PopModalAsync();
            _ = await Shell.Current.Navigation.PopAsync();

            if (Shell.Current.CurrentPage is CurrentTrainingPage currentPage)
            {
                (currentPage.BindingContext as CurrentTrainingPageViewModel)?.ReloadData();
            }
        }

        int parsedRepetitions = int.TryParse(LastRepetitions, out var lastReps) ? lastReps : 0;

        var model = IsDurationExercise
            ? CompleteExerciseModalViewModel.CreateDurationExercise(_trainingService,
                                                                    _exerciseParametersId,
                                                                    ExerciseName,
                                                                    _lastDurationInSeconds,
                                                                    onCompleted)
            : CompleteExerciseModalViewModel.CreateWeightExercise(_trainingService,
                                                                  _exerciseParametersId,
                                                                  ExerciseName,
                                                                  _lastWeight,
                                                                  parsedRepetitions,
                                                                  onCompleted);

        await Shell.Current.Navigation.PushModalAsync(new CompleteExerciseModal(model));
    }


    private static string BuildRestTimeLiteral(RoutineItemViewModel routineItem)
    {
        if (routineItem.ExerciseParameters.RestTimeInSeconds.Min == routineItem.ExerciseParameters.RestTimeInSeconds.Max)
        {
            return $"{routineItem.ExerciseParameters.RestTimeInSeconds.Min} segons";
        }
        else if (routineItem.ExerciseParameters.RestTimeInSeconds.Min == 0 && routineItem.ExerciseParameters.RestTimeInSeconds.Max > 0)
        {
            return $"{routineItem.ExerciseParameters.RestTimeInSeconds.Max} segons";
        }
        else if (routineItem.ExerciseParameters.RestTimeInSeconds.Max == 0 && routineItem.ExerciseParameters.RestTimeInSeconds.Min > 0)
        {
            return $"{routineItem.ExerciseParameters.RestTimeInSeconds.Min} segons";
        }

        return $"{routineItem.ExerciseParameters.RestTimeInSeconds.Min} - {routineItem.ExerciseParameters.RestTimeInSeconds.Max} segons";
    }
}


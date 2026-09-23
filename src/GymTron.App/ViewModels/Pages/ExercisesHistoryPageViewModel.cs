using GymTron.App.Services;
using GymTron.App.ViewModels.Entities;
using System.Collections.ObjectModel;

namespace GymTron.App.ViewModels.Pages;

public partial class ExercisesHistoryPageViewModel : PageBaseViewModel
{
    private readonly IExerciseService _exerciseService;

    public string Title => LocalizationService.GetString("ExercisesHistory_Title");
    public string DateFormat => LocalizationService.GetString("ExercisesHistory_Date");
    public string WeightFormat => LocalizationService.GetString("ExercisesHistory_Weight");
    public string RepsFormat => LocalizationService.GetString("ExercisesHistory_Reps");
    public string DurationFormat => LocalizationService.GetString("ExercisesHistory_Duration");

    private ObservableCollection<string> _distinctExerciseNames;
    public ObservableCollection<string> DistinctExerciseNames
    {
        get => _distinctExerciseNames;
        set => SetProperty(ref _distinctExerciseNames, value);
    }

    private string _selectedExerciseName;
    public string SelectedExerciseName
    {
        get => _selectedExerciseName;
        set
        {
            SetProperty(ref _selectedExerciseName, value);
            _ = LoadExerciseExecutionsAsync();
        }
    }

    private ObservableCollection<ExerciseViewModel> _selectedExerciseExecutions;
    public ObservableCollection<ExerciseViewModel> SelectedExerciseExecutions
    {
        get => _selectedExerciseExecutions;
        set => SetProperty(ref _selectedExerciseExecutions, value);
    }

    private bool _exercisesLoaded;
    public bool ExercisesLoaded
    {
        get => _exercisesLoaded;
        set => SetProperty(ref _exercisesLoaded, value);
    }


    public ExercisesHistoryPageViewModel(IExerciseService exerciseService)
    {
        _exerciseService = exerciseService;

        _ = LoadDistinctExerciseNamesAsync();
    }


    private async Task LoadDistinctExerciseNamesAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            List<ExerciseViewModel> exercises = await _exerciseService.ListAll();
            DistinctExerciseNames = new ObservableCollection<string>(exercises.Select(e => e.Name).Distinct());
            ExercisesLoaded = true;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            // Session expired; AuthHttpMessageHandler navigates to Login, suppress alert
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load exercises: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }


    private async Task LoadExerciseExecutionsAsync()
    {
        if (string.IsNullOrEmpty(SelectedExerciseName) || IsBusy)
            return;

        try
        {
            IsBusy = true;
            List<ExerciseViewModel> exercises = await _exerciseService.ListAll();
            List<ExerciseViewModel> filteredExercises = [.. exercises
                .Where(e => e.Name == SelectedExerciseName)
                .OrderByDescending(e => e.CreatedOn)];

            SelectedExerciseExecutions = new ObservableCollection<ExerciseViewModel>(filteredExercises);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            // Session expired; AuthHttpMessageHandler navigates to Login, suppress alert
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load exercise executions: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}

using System.Windows.Input;
using GymTron.App.Helpers;
using GymTron.App.Pages;
using GymTron.App.Services;
using GymTron.App.Services.Auth;
using GymTron.App.ViewModels.Entities;

namespace GymTron.App.ViewModels.Pages;

public partial class MainPageViewModel : PageBaseViewModel
{
    private readonly ITrainingService _trainingService;
    private readonly ITokenStorage _tokenStorage;
    private string _loggedUsername = string.Empty;

    public string LoggedUsername
    {
        get => _loggedUsername;
        set
        {
            if (SetProperty(ref _loggedUsername, value))
            {
                OnPropertyChanged(nameof(UserFooterText));
            }
        }
    }

    public string UserFooterText => string.IsNullOrWhiteSpace(LoggedUsername)
        ? string.Empty
        : $"{LocalizationService.GetString("MainPage_UserPrefix")}{LoggedUsername}";

    public string TrainingText => LocalizationService.GetString("MainPage_Training");
    public string TrainingHistoryText => LocalizationService.GetString("MainPage_TrainingHistory");
    public string ExerciseHistoryText => LocalizationService.GetString("MainPage_ExerciseHistory");
    public string BodyWeightHistoryText => LocalizationService.GetString("MainPage_Measures");
    public string SettingsText => LocalizationService.GetString("MainPage_Settings");

    public ICommand NavigateTrainingCommand { get; }
    public ICommand NavigateTrainingHistoryCommand { get; }
    public ICommand NavigateExerciseHistoryCommand { get; }
    public ICommand NavigateBodyWeightHistoryCommand { get; }
    public ICommand NavigateSettingsCommand { get; }

    public MainPageViewModel(ITrainingService trainingService, ITokenStorage tokenStorage)
    {
        _trainingService = trainingService;
        _tokenStorage = tokenStorage;

        LocalizationService.Initialize();
        LocalizationService.CultureChanged += OnCultureChanged;

        NavigateTrainingCommand = new Command(async () => await NavigateTrainingAsync());
        NavigateTrainingHistoryCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(TrainingsHistoryPage)));
        NavigateExerciseHistoryCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(ExercisesHistoryPage)));
        NavigateBodyWeightHistoryCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(BodyWeightsHistoryPage)));
        NavigateSettingsCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(SettingsPage)));

        _ = LoadLoggedUserAsync();
    }

    public async Task LoadLoggedUserAsync()
    {
        try
        {
            string? token = await _tokenStorage.GetAccessTokenAsync();
            string? username = JwtClaimsHelper.GetUsernameFromToken(token);
            if (!string.IsNullOrWhiteSpace(username))
            {
                LoggedUsername = username;
            }
        }
        catch
        {
            // Non-critical background load
        }
    }

    private void OnCultureChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(TrainingText));
        OnPropertyChanged(nameof(TrainingHistoryText));
        OnPropertyChanged(nameof(ExerciseHistoryText));
        OnPropertyChanged(nameof(BodyWeightHistoryText));
        OnPropertyChanged(nameof(SettingsText));
        OnPropertyChanged(nameof(UserFooterText));
    }

    private async Task NavigateTrainingAsync()
    {
        try
        {
            TrainingViewModel? currentTraining = await _trainingService.GetCurrentTraining();
            if (currentTraining != null)
            {
                await Shell.Current.GoToAsync(nameof(CurrentTrainingPage));
            }
            else
            {
                await Shell.Current.GoToAsync(nameof(StartTrainingPage));
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to navigate: {ex.Message}", "OK");
        }
    }
}

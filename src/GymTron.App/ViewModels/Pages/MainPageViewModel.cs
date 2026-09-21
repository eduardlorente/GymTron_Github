using System.Globalization;
using System.Windows.Input;
using GymTron.App.Pages;
using GymTron.App.Services;
using GymTron.App.ViewModels.Entities;

namespace GymTron.App.ViewModels.Pages;

public partial class MainPageViewModel : PageBaseViewModel
{
    private readonly ITrainingService _trainingService;
    private readonly List<CultureInfo> _cultures;
    private int _selectedLanguageIndex;

    public List<string> LanguageNames { get; }

    public int SelectedLanguageIndex
    {
        get => _selectedLanguageIndex;
        set
        {
            if (SetProperty(ref _selectedLanguageIndex, value) && value >= 0 && value < _cultures.Count)
            {
                LocalizationService.SetCulture(_cultures[value]);
            }
        }
    }

    public string LanguageTitle => LocalizationService.GetString("Language_Title");
    public string TrainingText => LocalizationService.GetString("MainPage_Training");
    public string TrainingHistoryText => LocalizationService.GetString("MainPage_TrainingHistory");
    public string ExerciseHistoryText => LocalizationService.GetString("MainPage_ExerciseHistory");
    public string BodyWeightHistoryText => LocalizationService.GetString("MainPage_Measures");

    public ICommand NavigateTrainingCommand { get; }
    public ICommand NavigateTrainingHistoryCommand { get; }
    public ICommand NavigateExerciseHistoryCommand { get; }
    public ICommand NavigateBodyWeightHistoryCommand { get; }

    public MainPageViewModel(ITrainingService trainingService)
    {
        _trainingService = trainingService;

        LocalizationService.Initialize();
        _cultures = LocalizationService.GetSupportedCultures();
        LanguageNames = _cultures.Select(LocalizationService.GetLanguageDisplayName).ToList();

        int currentIndex = _cultures.FindIndex(c => c.Name == LocalizationService.CurrentCulture.Name);
        _selectedLanguageIndex = currentIndex >= 0 ? currentIndex : 0;

        LocalizationService.CultureChanged += OnCultureChanged;

        NavigateTrainingCommand = new Command(async () => await NavigateTrainingAsync());
        NavigateTrainingHistoryCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(TrainingsHistoryPage)));
        NavigateExerciseHistoryCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(ExercisesHistoryPage)));
        NavigateBodyWeightHistoryCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(BodyWeightsHistoryPage)));
    }

    private void OnCultureChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(LanguageTitle));
        OnPropertyChanged(nameof(TrainingText));
        OnPropertyChanged(nameof(TrainingHistoryText));
        OnPropertyChanged(nameof(ExerciseHistoryText));
        OnPropertyChanged(nameof(BodyWeightHistoryText));
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

using GymTron.App.Pages;
using GymTron.App.Services;
using GymTron.App.ViewModels.Entities;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace GymTron.App.ViewModels.Pages;

public partial class StartTrainingPageViewModel : PageBaseViewModel
{
    private readonly ITrainingService _trainingService;
    private readonly IRoutineService _routineService;

    public string Title => LocalizationService.GetString("StartTraining_Title");

    private ObservableCollection<RoutineViewModel> _routines = [];
    public ObservableCollection<RoutineViewModel> Routines {
        get => _routines;
        set => SetProperty(ref _routines, value);
    }

    private ObservableCollection<int> _routineDays = [];
    public ObservableCollection<int> RoutineDays {
        get => _routineDays;
        set => SetProperty(ref _routineDays, value);
    }

    private ObservableCollection<RoutineItemViewModel> _routineItems = [];
    public ObservableCollection<RoutineItemViewModel> RoutineItems {
        get => _routineItems;
        set => SetProperty(ref _routineItems, value);
    }
 
    private RoutineViewModel? _selectedRoutine = null;
    public RoutineViewModel? SelectedRoutine
    {
        get => _selectedRoutine;
        set
        {
            SetProperty(ref _selectedRoutine, value);
            UpdateRoutineDays();
        }
    }

    private int? _selectedDay;
    public int? SelectedDay
    {
        get => _selectedDay;
        set
        {
            SetProperty(ref _selectedDay, value);
            UpdateRoutineItems();
        }
    }

    private bool _routinesLoaded;
    public bool RoutinesLoaded
    {
        get => _routinesLoaded;
        set => SetProperty(ref _routinesLoaded, value);
    }

    public ICommand StartTrainingCommand { get; }
    public ICommand LoadRoutinesCommand { get; }


    public StartTrainingPageViewModel(ITrainingService trainingService, IRoutineService routineService)
    {
        _trainingService = trainingService;
        _routineService = routineService;

        StartTrainingCommand = new Command(async () =>
        {
            IsBusy = true;
            try
            {
                await OnStartTraining();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to start training: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        });

        LoadRoutinesCommand = new Command(async () =>
        {
            IsBusy = true;
            try
            {
                await LoadRoutines();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load routines: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        });
    }


    private async Task LoadRoutines()
    {
        try
        {
            List<RoutineViewModel> routines = await _routineService.ListAllRoutines();
            
            Routines.Clear();
            Routines = new ObservableCollection<RoutineViewModel>(routines);
            RoutinesLoaded = true;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load routines: {ex.Message}", "OK");
        }
    }


    private void UpdateRoutineDays()
    {
        SelectedDay = null;
        RoutineDays.Clear();
        RoutineItems.Clear();
        if (SelectedRoutine != null)
        {
            foreach (int day in SelectedRoutine.WorkByDays.Keys)
                RoutineDays.Add(day);
        }
    }


    private void UpdateRoutineItems()
    {
        RoutineItems.Clear();
        if (SelectedRoutine != null && SelectedDay.HasValue &&
            SelectedRoutine.WorkByDays.TryGetValue(SelectedDay.Value, out var items))
        {
            foreach (RoutineItemViewModel item in items)
                RoutineItems.Add(item);
        }
    }


    private async Task OnStartTraining()
    {
        if (SelectedRoutine == null || SelectedDay == null)
            return;

        try
        {
            await _trainingService.StartTraining(SelectedRoutine.Id, SelectedDay.Value);

            await Shell.Current.GoToAsync(nameof(CurrentTrainingPage));

            var nav = Shell.Current?.Navigation;
            if (nav != null && nav.NavigationStack.Count > 1)
            {
                var previousPage = nav.NavigationStack[nav.NavigationStack.Count - 2];
                if (previousPage != null)
                {
                    nav.RemovePage(previousPage);
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to start training: {ex.Message}", "OK");
        }
    }
}

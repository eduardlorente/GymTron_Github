using GymTron.App.Services;
using System.Globalization;
using System.Windows.Input;

namespace GymTron.App.ViewModels.Pages.Modals;

public class CompleteExerciseModalViewModel : PageBaseViewModel
{


    private const string HARD_OBSERVATION = "Ha estat dur";
    private const string REALLY_HARD_OBSERVATION = "Ha estat molt dur";
    private const string INCREASE_WEIGHT_OBSERVATION = "Pujar pes";
    private const string INCREASE_REPETITIONS_OBSERVATION = "Pujar repeticions";


    public bool IsDurationExercise { get; set; }
    public bool IsWeightExercise => !IsDurationExercise;

    private string _duration = string.Empty;
    public string Duration
    {
        get => _duration;
        set => SetProperty(ref _duration, value);
    }

    private string _weight = string.Empty;
    public string Weight
    {
        get => _weight;
        set => SetProperty(ref _weight, value);
    }

    private string _repetitions = string.Empty;
    public string Repetitions
    {
        get => _repetitions;
        set => SetProperty(ref _repetitions, value);
    }
    public bool SelectedEasyDifficulty { get; set; }
    public bool SelectedRegularDifficulty { get; set; }
    public bool SelectedHardDifficulty { get; set; }
    public bool SelectedReallyHardDifficulty { get; set; }
    public bool IncreaseWeight { get; set; }
    public bool IncreaseRepetitions { get; set; }
    public string Observations { get; set; } = string.Empty;

    public ICommand CompleteCommand { get; }

    private readonly ITrainingService _trainingService;
    private readonly int _exerciseParametersId;
    private readonly string _name = string.Empty;
    private readonly Func<Task> _onCompleted;

    private CompleteExerciseModalViewModel(ITrainingService trainingService,
                                           int exerciseParametersId,
                                           string name,
                                           decimal previousWeight,
                                           int previousRepetitions,
                                           int previousDuration,
                                           bool isDurationExercise,
                                           Func<Task> onCompleted)
    {
        _trainingService = trainingService;
        _exerciseParametersId = exerciseParametersId;
        _name = name;
        IsDurationExercise = isDurationExercise;
        Weight = previousWeight > 0 ? previousWeight.ToString(CultureInfo.InvariantCulture) : string.Empty;
        Repetitions = previousRepetitions > 0 ? previousRepetitions.ToString() : string.Empty;
        Duration = previousDuration > 0 ? previousDuration.ToString() : string.Empty;
        _onCompleted = onCompleted;

        CompleteCommand = new Command(async () => await OnComplete());
    }

    public static CompleteExerciseModalViewModel CreateWeightExercise(ITrainingService trainingService,
                                                                      int exerciseParametersId,
                                                                      string name,
                                                                      decimal previousWeight,
                                                                      int previousRepetitions,
                                                                      Func<Task> onCompleted)
        => new(trainingService,
               exerciseParametersId,
               name,
               previousWeight,
               previousRepetitions,
               0,
               false,
               onCompleted);

    public static CompleteExerciseModalViewModel CreateDurationExercise(ITrainingService trainingService,
                                                                        int exerciseParametersId,
                                                                        string name,
                                                                        int previousDuration,
                                                                        Func<Task> onCompleted)
        => new(trainingService,
               exerciseParametersId,
               name,
               0,
               0,
               previousDuration,
               true,
               onCompleted);

    private async Task OnComplete()
    {
        if (IsDurationExercise)
        {
            if (!int.TryParse(Duration?.Trim(), out int durationInSeconds) || durationInSeconds <= 0)
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert(
                        LocalizationService.GetString("Dialog_Alert"),
                        LocalizationService.GetString("CompleteModal_InvalidDuration"),
                        LocalizationService.GetString("Dialog_Ok"));
                }
                return;
            }

            await _trainingService.CompleteTrainingExercise(_exerciseParametersId, _name, durationInSeconds, GetObservations());
        }
        else
        {
            bool hasValidWeight = TryParseDecimal(Weight, out decimal weight) && weight > 0;
            bool hasValidReps = int.TryParse(Repetitions?.Trim(), out int repetitions) && repetitions > 0;

            if (!hasValidWeight || !hasValidReps)
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert(
                        LocalizationService.GetString("Dialog_Alert"),
                        LocalizationService.GetString("CompleteModal_InvalidWeightOrReps"),
                        LocalizationService.GetString("Dialog_Ok"));
                }
                return;
            }

            await _trainingService.CompleteTrainingExercise(_exerciseParametersId, _name, weight, repetitions, GetObservations());
        }

        await _onCompleted();
    }

    private static bool TryParseDecimal(string? input, out decimal result)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            result = 0;
            return false;
        }

        string normalized = input.Trim().Replace(',', '.');
        return decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }


    private List<string> GetObservations()
    {
        List<string> observations = [];

        if (SelectedHardDifficulty)
        {
            observations.Add(HARD_OBSERVATION);
        }
        else if (SelectedReallyHardDifficulty)
        {
            observations.Add(REALLY_HARD_OBSERVATION);
        }

        if (!string.IsNullOrWhiteSpace(Observations))
        {
            observations.Add(Observations);
        }

        if (IncreaseWeight)
        {
            observations.Add(INCREASE_WEIGHT_OBSERVATION);
        }

        if (IncreaseRepetitions)
        {
            observations.Add(INCREASE_REPETITIONS_OBSERVATION);
        }

        return observations;
    }
}


using GymTron.App.Services;
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
    public string Duration { get; set; } = string.Empty;
    public string Weight { get; set; } = string.Empty;
    public string Repetitions { get; set; } = string.Empty;
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
        Weight = previousWeight.ToString();
        Repetitions = previousRepetitions.ToString();
        Duration = previousDuration.ToString();
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
        if (IsDurationExercise && int.TryParse(Duration, out int durationInSeconds))
        {
            await _trainingService.CompleteTrainingExercise(_exerciseParametersId, _name, durationInSeconds, GetObservations());
        }
        else if (decimal.TryParse(Weight, out decimal weight) && int.TryParse(Repetitions, out int repetitions))
        {
            await _trainingService.CompleteTrainingExercise(_exerciseParametersId, _name, weight, repetitions, GetObservations());
        }

        await _onCompleted();
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


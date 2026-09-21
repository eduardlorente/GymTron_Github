using GymTron.App.ViewModels.Entities;

namespace GymTron.App.Services;

public interface ITrainingService
{
    Task StartTraining(int routineId, int dayOfWeek);
    Task<TrainingViewModel?> GetCurrentTraining();
    Task FinalizeTraining();
    Task CompleteTrainingExercise(int exerciseParametersId, string name, decimal weight, int repetitions, List<string> observations);
    Task CompleteTrainingExercise(int exerciseParametersId, string name, int durationInSeconds, List<string> observations);
    Task<List<TrainingHistoryItemViewModel>> ListHistory();
    Task CancelTraining();
}
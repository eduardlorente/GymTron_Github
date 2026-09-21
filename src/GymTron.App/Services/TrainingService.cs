using GymTron.App.Helpers;
using GymTron.App.Services.Api;
using GymTron.App.Services.Api.Models;
using GymTron.App.ViewModels.Entities;
using Newtonsoft.Json;

namespace GymTron.App.Services;

internal class TrainingService(IGymTronApiClient apiClient) : ITrainingService
{
    private const string CURRENT_TRAINING_PREFERENCES_KEY = "current_training";

    private readonly IGymTronApiClient _apiClient = apiClient;

    private TrainingViewModel? _currentTraining;

    public async Task StartTraining(int routineId, int dayOfWeek)
    {
        if (_currentTraining != null)
        {
            throw new InvalidOperationException("There is already a training in progress.");
        }

        await _apiClient.StartTrainingAsync(routineId, dayOfWeek);

        SaveCurrentTrainingIntoMemory(await GetCurrentTraining());
    }

    public async Task<TrainingViewModel?> GetCurrentTraining()
    {
        if (_currentTraining != null)
        {
            return _currentTraining;
        }

        _currentTraining = GetTrainingFromPreferences();

        if (_currentTraining == null)
        {
            var training = await _apiClient.GetCurrentTrainingAsync();

            if (training != null)
            {
                SaveCurrentTrainingIntoMemory(new TrainingViewModel(training));
            }
        }

        return _currentTraining;
    }

    public async Task FinalizeTraining()
    {
        if (_currentTraining == null)
        {
            throw new InvalidOperationException("There is no training in progress.");
        }

        await _apiClient.FinishTrainingAsync(_currentTraining.ToDto());

        RemoveCurrentTraining();
    }

    public async Task CancelTraining()
    {
        if (_currentTraining == null)
        {
            throw new InvalidOperationException("There is no training in progress.");
        }

        await _apiClient.CancelTrainingAsync(_currentTraining.ToDto());

        RemoveCurrentTraining();
    }

    public async Task CompleteTrainingExercise(int exerciseParametersId, string name, decimal weight, int repetitions, List<string> observations)
    {
        if (_currentTraining == null)
        {
            throw new InvalidOperationException("There is no training in progress.");
        }

        var request = new AddExerciseToTrainingRequest(
            _currentTraining.ToDto(),
            exerciseParametersId,
            name,
            weight,
            repetitions,
            null,
            observations);

        var updatedTraining = await _apiClient.AddExerciseToTrainingAsync(request);

        if (updatedTraining != null)
        {
            SaveCurrentTrainingIntoMemory(new TrainingViewModel(updatedTraining));
        }
    }

    public async Task CompleteTrainingExercise(int exerciseParametersId, string name, int durationInSeconds, List<string> observations)
    {
        if (_currentTraining == null)
        {
            throw new InvalidOperationException("There is no training in progress.");
        }

        var request = new AddExerciseToTrainingRequest(
            _currentTraining.ToDto(),
            exerciseParametersId,
            name,
            null,
            null,
            durationInSeconds,
            observations);

        var updatedTraining = await _apiClient.AddExerciseToTrainingAsync(request);

        if (updatedTraining != null)
        {
            SaveCurrentTrainingIntoMemory(new TrainingViewModel(updatedTraining));
        }
    }

    public async Task<List<TrainingHistoryItemViewModel>> ListHistory()
    {
        var trainings = await _apiClient.GetTrainingHistoryAsync();

        List<TrainingHistoryItemViewModel> groupedByMonth = [.. trainings.GroupBy(t => new { t.StartedOn.Year, t.StartedOn.Month })
                                                                         .OrderByDescending(g => g.Key.Year)
                                                                         .ThenByDescending(g => g.Key.Month)
                                                                         .Select(g =>
                                                                         {
                                                                             Dictionary<int, int> trainingDays = g
                                                                                 .OrderBy(k => k.DayOfTheWeek)
                                                                                 .GroupBy(t => t.DayOfTheWeek)
                                                                                 .ToDictionary(dg => dg.Key, dg => dg.Count());

                                                                             return new TrainingHistoryItemViewModel(g.Key.Year, DateUtils.MonthName(g.Key.Month), g.Count(), trainingDays);
                                                                         })];

        return groupedByMonth;
    }

    private void SaveCurrentTrainingIntoMemory(TrainingViewModel? training)
    {
        _currentTraining = training;

        string trainingJson = JsonConvert.SerializeObject(training);
        Preferences.Set(CURRENT_TRAINING_PREFERENCES_KEY, trainingJson);
    }

    private void RemoveCurrentTraining()
    {
        _currentTraining = null;

        Preferences.Remove(CURRENT_TRAINING_PREFERENCES_KEY);
    }

    private static TrainingViewModel? GetTrainingFromPreferences()
    {
        if (Preferences.ContainsKey(CURRENT_TRAINING_PREFERENCES_KEY))
        {
            string trainingJson = Preferences.Get(CURRENT_TRAINING_PREFERENCES_KEY, string.Empty);
            if (!string.IsNullOrEmpty(trainingJson))
            {
                return JsonConvert.DeserializeObject<TrainingViewModel>(trainingJson);
            }
        }

        return null;
    }
}

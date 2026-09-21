using GymTron.App.ViewModels.Entities;

namespace GymTron.App.Services;

public interface IExerciseService
{
    Task<List<ExerciseViewModel>> ListAll();
}

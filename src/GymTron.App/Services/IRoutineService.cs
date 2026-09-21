using GymTron.App.ViewModels.Entities;

namespace GymTron.App.Services;

public interface IRoutineService
{
    Task<List<RoutineViewModel>> ListAllRoutines();
}
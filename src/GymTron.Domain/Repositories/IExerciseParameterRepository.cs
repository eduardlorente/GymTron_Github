using GymTron.Domain.Entities;
using GymTron.Domain.Projections;

namespace GymTron.Domain.Repositories;

public interface IExerciseParameterRepository
{
    Task<List<ExerciseParameters>> ListAll(CancellationToken cancellationToken = default);
    Task<ExerciseParameters?> GetById(int id, CancellationToken cancellationToken = default);
    Task<ExerciseParameterProjection?> GetProjection(int id, CancellationToken cancellationToken = default);
    Task<List<ExerciseParameterProjection>> ListProjections(CancellationToken cancellationToken = default);
    Task<int> Create(ExerciseParameters exerciseParameters, CancellationToken cancellationToken = default);
    Task Update(ExerciseParameters exerciseParameters, CancellationToken cancellationToken = default);
}

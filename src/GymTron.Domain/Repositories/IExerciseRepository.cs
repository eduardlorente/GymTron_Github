using GymTron.Domain.Entities;
using GymTron.Domain.Projections;

namespace GymTron.Domain.Repositories;

public interface IExerciseRepository : IRepository<Exercise>
{
    Task Add(Exercise entity, CancellationToken cancellationToken = default);
    Task AddRange(IReadOnlyCollection<Exercise> exercises, CancellationToken cancellationToken = default);
    Task<List<Exercise>> ListAll(CancellationToken cancellationToken = default);
    Task<List<Exercise>> ListByTraining(int trainingId, CancellationToken cancellationToken = default);
    Task<List<ExerciseHistoryProjection>> ListHistory(int? userId = null, CancellationToken cancellationToken = default);
}

using GymTron.Domain.Aggregates;
using GymTron.Domain.Projections;

namespace GymTron.Domain.Repositories;

public interface ITrainingRepository : IEntityRepository<Training, int>
{
    Task<Training?> GetCurrent(int? userId = null, CancellationToken cancellationToken = default);
    Task<List<Training>> ListAllWithoutExercises(CancellationToken cancellationToken = default);
    Task<List<TrainingHistoryProjection>> ListCompletedHistory(int? userId = null, CancellationToken cancellationToken = default);
}

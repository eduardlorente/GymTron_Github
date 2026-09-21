using GymTron.Domain.Entities;
using GymTron.Infrastructure.Persistence.DAL.Models;

namespace GymTron.Infrastructure.Persistence.DAL.MySQL;

internal interface IExerciseDAL
{
    Task Add(Exercise entity, CancellationToken cancellationToken = default);
    Task AddRange(List<Exercise> exercises, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExerciseDALModel>> ListByTrainingId(int trainingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExerciseDALModel>> ListAll(int? userId = null, CancellationToken cancellationToken = default);
}
using GymTron.Domain.Aggregates;
using GymTron.Infrastructure.Persistence.DAL.Models;

namespace GymTron.Infrastructure.Persistence.DAL.MySQL;

internal interface ITrainingDAL
{
    Task Add(Training entity, CancellationToken cancellationToken = default);
    Task<TrainingDALModel?> GetById(int id, CancellationToken cancellationToken = default);
    Task<TrainingDALModel?> GetCurrent(int? userId = null, CancellationToken cancellationToken = default);
    Task<List<TrainingDALModel>> ListAll(int? userId = null, CancellationToken cancellationToken = default);
    Task Update(TrainingDALModel model, CancellationToken cancellationToken = default);
}
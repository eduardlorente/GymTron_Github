using GymTron.Domain.Entities;
using GymTron.Domain.Projections;

namespace GymTron.Domain.Repositories;

public interface IBodyWeightRepository : IRepository<BodyWeight>
{
    Task Add(BodyWeight entity, CancellationToken cancellationToken = default);
    Task<List<BodyWeight>> ListAll(CancellationToken cancellationToken = default);
    Task<List<BodyWeightHistoryProjection>> ListHistory(int? userId = null, CancellationToken cancellationToken = default);
}

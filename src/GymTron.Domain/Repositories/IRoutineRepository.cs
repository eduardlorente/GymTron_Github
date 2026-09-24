using GymTron.Domain.Entities;
using GymTron.Domain.Projections;

namespace GymTron.Domain.Repositories;

public interface IRoutineRepository : IRepository<Routine>
{
    Task<List<Routine>> ListAll(int? userId = null, CancellationToken cancellationToken = default);
    Task<Routine?> GetById(int id, int? userId = null, CancellationToken cancellationToken = default);
    Task<RoutineProjection?> GetRoutineProjection(int id, int? userId = null, CancellationToken cancellationToken = default);
    Task<List<RoutineProjection>> ListRoutineProjections(int? userId = null, CancellationToken cancellationToken = default);
    Task<int> Create(Routine routine, CancellationToken cancellationToken = default);
    Task Update(Routine routine, CancellationToken cancellationToken = default);
}

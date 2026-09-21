using GymTron.Domain.Entities;

namespace GymTron.Domain.Repositories;

public interface ILogRepository : IRepository<Log>
{
    Task Add(Log entity, CancellationToken cancellationToken = default);
}

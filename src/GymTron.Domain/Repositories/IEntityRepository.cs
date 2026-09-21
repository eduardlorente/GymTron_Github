using GymTron.Domain.Entities;

namespace GymTron.Domain.Repositories;

public interface IEntityRepository<T, TId> : IRepository<T>
    where T : Entity<TId>
{
    Task<T?> GetById(TId id, CancellationToken cancellationToken = default);
    Task Add(T entity, CancellationToken cancellationToken = default);
    Task Update(T entity, CancellationToken cancellationToken = default);
}

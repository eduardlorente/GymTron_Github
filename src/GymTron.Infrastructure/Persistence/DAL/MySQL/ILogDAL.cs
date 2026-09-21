using GymTron.Domain.Entities;

namespace GymTron.Infrastructure.Persistence.DAL.MySQL;

internal interface ILogDAL
{
    Task Add(Log entity, CancellationToken cancellationToken = default);
}
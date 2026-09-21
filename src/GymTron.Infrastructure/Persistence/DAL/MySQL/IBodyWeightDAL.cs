using GymTron.Domain.Entities;
using GymTron.Infrastructure.Persistence.DAL.Models;

namespace GymTron.Infrastructure.Persistence.DAL.MySQL;

internal interface IBodyWeightDAL
{
    Task Add(BodyWeight entity, CancellationToken cancellationToken = default);
    Task<IEnumerable<BodyWeightDALModel>> ListAll(int? userId = null, CancellationToken cancellationToken = default);
}

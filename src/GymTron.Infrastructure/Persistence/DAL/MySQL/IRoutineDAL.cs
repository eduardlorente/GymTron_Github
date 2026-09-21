using GymTron.Domain.Enums;
using GymTron.Infrastructure.Persistence.DAL.Models;

namespace GymTron.Infrastructure.Persistence.DAL.MySQL;

internal interface IRoutineDAL
{
    Task<IEnumerable<RoutineFullDetailsDTO>> ListAll(int? userId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<RoutineFullDetailsDTO>> ListById(int id, CancellationToken cancellationToken = default);
    Task<int> Create(string name, IReadOnlyList<RoutineItemWriteModel> items, int? userId = null, CancellationToken cancellationToken = default);
    Task Update(int id, string name, IReadOnlyList<RoutineItemWriteModel> items, CancellationToken cancellationToken = default);
}

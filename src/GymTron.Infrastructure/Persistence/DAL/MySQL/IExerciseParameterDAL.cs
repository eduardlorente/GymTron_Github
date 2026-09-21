using GymTron.Infrastructure.Persistence.DAL.Models;

namespace GymTron.Infrastructure.Persistence.DAL.MySQL;

internal interface IExerciseParameterDAL
{
    Task<IEnumerable<ExerciseParameterDALModel>> ListAll(CancellationToken cancellationToken = default);
    Task<ExerciseParameterDALModel?> GetById(int id, CancellationToken cancellationToken = default);
    Task<int> Create(string name, string description, string pattern, int typeId, int? replaysInReserve, CancellationToken cancellationToken = default);
    Task Update(int id, string name, string description, string pattern, int typeId, int? replaysInReserve, CancellationToken cancellationToken = default);
}

using GymTron.Domain.Entities;
using GymTron.Domain.Repositories;
using GymTron.Infrastructure.Persistence.DAL.MySQL;

namespace GymTron.Infrastructure.Persistence.Repositories;

internal class LogRepository(ILogDAL logDAL) : ILogRepository
{


    private readonly ILogDAL _logDAL = logDAL;


    public async Task Add(Log entity, CancellationToken cancellationToken = default)
    {
        await _logDAL.Add(entity, cancellationToken);
    }
}
